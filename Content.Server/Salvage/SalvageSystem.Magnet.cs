// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later or MIT

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Salvage.Magnet;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Parallax.Biomes;
using Content.Shared.Procedural;
using Content.Shared.Radio;
using Content.Shared.Salvage;
using Content.Shared.Salvage.Magnet;
using Robust.Shared.Collections;
using Robust.Shared.Exceptions;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Salvage;

public sealed partial class SalvageSystem
{

    private static readonly ProtoId<RadioChannelPrototype> MagnetChannel = "Supply";

    [Dependency] private readonly SalvageRuinGeneratorSystem _ruinGenerator = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    private EntityQuery<SalvageMobRestrictionsComponent> _salvMobQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    private List<(Entity<TransformComponent> Entity, EntityUid MapUid, Vector2 LocalPosition)> _detachEnts = new();

    private void InitializeMagnet()
    {
        _salvMobQuery = GetEntityQuery<SalvageMobRestrictionsComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<SalvageMagnetDataComponent, MapInitEvent>(OnMagnetDataMapInit);

        SubscribeLocalEvent<SalvageMagnetTargetComponent, GridSplitEvent>(OnMagnetTargetSplit);

        SubscribeLocalEvent<SalvageMagnetComponent, MagnetClaimOfferEvent>(OnMagnetClaim);
        SubscribeLocalEvent<SalvageMagnetComponent, ComponentStartup>(OnMagnetStartup);
        SubscribeLocalEvent<SalvageMagnetComponent, AnchorStateChangedEvent>(OnMagnetAnchored);
    }

    private void OnMagnetClaim(EntityUid uid, SalvageMagnetComponent component, ref MagnetClaimOfferEvent args)
    {
        var station = _station.GetOwningStation(uid);

        if (!TryComp(station, out SalvageMagnetDataComponent? dataComp) ||
            dataComp.EndTime != null)
        {
            return;
        }

        var index = args.Index;
        // Fire-and-forget async call - this is intentional for event handlers
        // The async work needs to run on the main thread with entity context
        _ = TakeMagnetOffer((station.Value, dataComp), index, (uid, component));
    }

    private void OnMagnetStartup(EntityUid uid, SalvageMagnetComponent component, ComponentStartup args)
    {
        UpdateMagnetUI((uid, component), Transform(uid));
    }

    private void OnMagnetAnchored(EntityUid uid, SalvageMagnetComponent component, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        UpdateMagnetUI((uid, component), args.Transform);
    }

    private void OnMagnetDataMapInit(EntityUid uid, SalvageMagnetDataComponent component, ref MapInitEvent args)
    {
        CreateMagnetOffers((uid, component));
    }

    private void OnMagnetTargetSplit(EntityUid uid, SalvageMagnetTargetComponent component, ref GridSplitEvent args)
    {
        // Don't think I'm not onto you people splitting to make new grids.
        if (TryComp(component.DataTarget, out SalvageMagnetDataComponent? dataComp))
        {
            foreach (var gridUid in args.NewGrids)
            {
                dataComp.ActiveEntities?.Add(gridUid);
            }
        }
    }

    private void UpdateMagnet()
    {
        var dataQuery = EntityQueryEnumerator<SalvageMagnetDataComponent>();
        var curTime = _timing.CurTime;

        while (dataQuery.MoveNext(out var uid, out var magnetData))
        {
            // Magnet currently active.
            if (magnetData.EndTime != null)
            {
                if (magnetData.EndTime.Value < curTime)
                {
                    EndMagnet((uid, magnetData));
                }
                else if (!magnetData.Announced && (magnetData.EndTime.Value - curTime).TotalSeconds < 31)
                {
                    var magnet = GetMagnet((uid, magnetData));

                    if (magnet != null)
                    {
                        Report(magnet.Value.Owner, MagnetChannel,
                            "salvage-system-announcement-losing",
                            ("timeLeft", (magnetData.EndTime.Value - curTime).Seconds));
                    }

                    magnetData.Announced = true;
                }
            }
            if (magnetData.NextOffer < curTime)
            {
                CreateMagnetOffers((uid, magnetData));
            }
        }
    }

    /// <summary>
    /// Ends the magnet attachment and deletes the relevant grids.
    /// </summary>
    private void EndMagnet(Entity<SalvageMagnetDataComponent> data)
    {
        if (data.Comp.ActiveEntities != null)
        {
            // Handle mobrestrictions getting deleted
            var query = AllEntityQuery<SalvageMobRestrictionsComponent>();

            while (query.MoveNext(out var salvUid, out var salvMob))
            {
                if (data.Comp.ActiveEntities.Contains(salvMob.LinkedEntity))
                {
                    QueueDel(salvUid);
                }
            }

            // Uhh yeah don't delete mobs or whatever
            var mobQuery = AllEntityQuery<MobStateComponent, TransformComponent>();
            _detachEnts.Clear();

            while (mobQuery.MoveNext(out var mobUid, out _, out var xform))
            {
                if (xform.GridUid == null || !data.Comp.ActiveEntities.Contains(xform.GridUid.Value) || xform.MapUid == null)
                    continue;

                if (_salvMobQuery.HasComp(mobUid))
                    continue;

                bool CheckParents(EntityUid uid)
                {
                    do
                    {
                        uid = _transform.GetParentUid(uid);
                        if (_mobStateQuery.HasComp(uid))
                            return true;
                    }
                    while (uid != xform.GridUid && uid != EntityUid.Invalid);
                    return false;
                }

                if (CheckParents(mobUid))
                    continue;

                // Can't parent directly to map as it runs grid traversal.
                _detachEnts.Add(((mobUid, xform), xform.MapUid.Value, _transform.GetWorldPosition(xform)));
                _transform.DetachEntity(mobUid, xform);
            }

            // Go and cleanup the active ents.
            foreach (var ent in data.Comp.ActiveEntities)
            {
                Del(ent);
            }

            foreach (var entity in _detachEnts)
            {
                _transform.SetCoordinates(entity.Entity.Owner, new EntityCoordinates(entity.MapUid, entity.LocalPosition));
            }

            data.Comp.ActiveEntities = null;
        }

        data.Comp.EndTime = null;
        UpdateMagnetUIs(data);
    }

    private void CreateMagnetOffers(Entity<SalvageMagnetDataComponent> data)
    {
        data.Comp.Offered.Clear();

        for (var i = 0; i < data.Comp.OfferCount; i++)
        {
            var seed = _random.Next();

            // Fuck with the seed to mix wrecks and asteroids.
            seed = (int) (seed / 10f) * 10;


            if (i >= data.Comp.OfferCount / 2)
            {
                seed++;
            }


            data.Comp.Offered.Add(seed);
        }

        data.Comp.NextOffer = _timing.CurTime + data.Comp.OfferCooldown;
        UpdateMagnetUIs(data);
    }

    // Just need something to announce.
    private Entity<SalvageMagnetComponent>? GetMagnet(Entity<SalvageMagnetDataComponent> data)
    {
        var query = AllEntityQuery<SalvageMagnetComponent, TransformComponent>();

        while (query.MoveNext(out var magnetUid, out var magnet, out var xform))
        {
            var stationUid = _station.GetOwningStation(magnetUid, xform);

            if (stationUid != data.Owner)
                continue;

            return (magnetUid, magnet);
        }

        return null;
    }

    private void UpdateMagnetUI(Entity<SalvageMagnetComponent> entity, TransformComponent xform)
    {
        var station = _station.GetOwningStation(entity, xform);

        if (!TryComp(station, out SalvageMagnetDataComponent? dataComp))
            return;

        _ui.SetUiState(entity.Owner, SalvageMagnetUiKey.Key,
            new SalvageMagnetBoundUserInterfaceState(dataComp.Offered)
            {
                Cooldown = dataComp.OfferCooldown,
                Duration = dataComp.ActiveTime,
                EndTime = dataComp.EndTime,
                NextOffer = dataComp.NextOffer,
                ActiveSeed = dataComp.ActiveSeed,
            });
    }

    private void UpdateMagnetUIs(Entity<SalvageMagnetDataComponent> data)
    {
        var query = AllEntityQuery<SalvageMagnetComponent, TransformComponent>();

        while (query.MoveNext(out var magnetUid, out var magnet, out var xform))
        {
            var station = _station.GetOwningStation(magnetUid, xform);

            if (station != data.Owner)
                continue;

            _ui.SetUiState(magnetUid, SalvageMagnetUiKey.Key,
                new SalvageMagnetBoundUserInterfaceState(data.Comp.Offered)
                {
                    Cooldown = data.Comp.OfferCooldown,
                    Duration = data.Comp.ActiveTime,
                    EndTime = data.Comp.EndTime,
                    NextOffer = data.Comp.NextOffer,
                    ActiveSeed = data.Comp.ActiveSeed,
                });
        }
    }

    private async Task TakeMagnetOffer(Entity<SalvageMagnetDataComponent> data, int index, Entity<SalvageMagnetComponent> magnet)
    {
        var seed = data.Comp.Offered[index];

        var offering = GetSalvageOffering(seed);
        var salvMap = _mapSystem.CreateMap();
        var salvMapXform = Transform(salvMap);

        // Set values while awaiting asteroid dungeon if relevant so we can't double-take offers.
        data.Comp.ActiveSeed = seed;
        data.Comp.EndTime = _timing.CurTime + data.Comp.ActiveTime;
        data.Comp.NextOffer = data.Comp.EndTime.Value;
        UpdateMagnetUIs(data);

        // Get ruin configuration if this is a ruin offering (needed for placement later)
        SalvageMagnetRuinConfigPrototype? ruinConfig = null;
        if (offering is RuinOffering)
        {
            var ruinConfigId = new ProtoId<SalvageMagnetRuinConfigPrototype>("Default");
            if (!_prototypeManager.TryIndex(ruinConfigId, out ruinConfig))
            {
                // Try to get first available config as fallback
                ruinConfig = _prototypeManager.EnumeratePrototypes<SalvageMagnetRuinConfigPrototype>().FirstOrDefault();
            }
        }

        switch (offering)
        {
            case AsteroidOffering asteroid:
                var grid = _mapManager.CreateGridEntity(salvMap);
                await _dungeon.GenerateDungeonAsync(asteroid.DungeonConfig, grid.Owner, grid.Comp, Vector2i.Zero, seed);
                break;
            case DebrisOffering debris:
                var debrisProto = _prototypeManager.Index<DungeonConfigPrototype>(debris.Id);
                var debrisGrid = _mapManager.CreateGridEntity(salvMap);
                await _dungeon.GenerateDungeonAsync(debrisProto, debrisGrid.Owner, debrisGrid.Comp, Vector2i.Zero, seed);
                break;
            case SalvageOffering wreck:
                var salvageProto = wreck.SalvageMap;

                if (!_loader.TryLoadGrid(salvMapXform.MapID, salvageProto.MapPath, out _))
                {
                    Report(magnet, MagnetChannel, "salvage-system-announcement-spawn-debris-disintegrated");
                    _mapSystem.DeleteMap(salvMapXform.MapID);
                    return;
                }

                break;
            case RuinOffering ruin:
                // Generate single large ruin using multi-stage flood-fill
                var ruinResult = _ruinGenerator.GenerateRuin(ruin.RuinMap.MapPath, seed, ruinConfig);
                if (ruinResult == null)
                {
                    Report(magnet, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
                    _mapSystem.DeleteMap(salvMapXform.MapID);
                    return;
                }

                // Build complete grid in memory before using it
                // This ensures all tiles, walls, and windows are properly placed and anchored
                var ruinGrid = _mapManager.CreateGridEntity(salvMap);
                
                // STEP 1: Set all floor tiles (damage simulation already applied in GenerateRuin)
                _mapSystem.SetTiles(ruinGrid.Owner, ruinGrid.Comp, ruinResult.FloorTiles);
                
                // STEP 2: Spawn all wall entities (destruction already filtered in GenerateRuin)
                // Walls auto-anchor when spawned on a grid
                foreach (var (wallPos, wallProto) in ruinResult.WallEntities)
                {
                    var wallCoords = new EntityCoordinates(ruinGrid.Owner, wallPos);
                    SpawnAtPosition(wallProto, wallCoords);
                }
                
                // STEP 3: Spawn all window entities with preserved rotation
                // Always explicitly anchor windows to ensure they're properly anchored
                var windowDamageChance = ruinResult.Config?.WindowDamageChance ?? 0.0f;
                var windowRand = new System.Random(seed);
                
                foreach (var (windowPos, windowProto, windowRotation) in ruinResult.WindowEntities)
                {
                    // Check if tile exists at window position before spawning
                    // Windows can only anchor to non-empty tiles
                    var tileRef = _mapSystem.GetTileRef(ruinGrid.Owner, ruinGrid.Comp, windowPos);
                    if (tileRef.Tile.IsEmpty)
                    {
                        Log.Warning($"[SalvageSystem] Skipping window {windowProto} at {windowPos} - tile is empty");
                        continue;
                    }
                    
                    var windowCoords = new EntityCoordinates(ruinGrid.Owner, windowPos);
                    // Use SpawnAttachedTo to preserve rotation for directional windows
                    var windowEntity = SpawnAttachedTo(windowProto, windowCoords, rotation: windowRotation);
                    
                    // CRITICAL: Ensure windows are properly anchored immediately after spawning
                    // This ensures they are anchored before the grid is used
                    bool isAnchored = false;
                    var windowXform = Transform(windowEntity);
                    if (windowXform != null)
                    {
                        // Only anchor if not already anchored to avoid assertion failures
                        // SpawnAttachedTo may already anchor some entities
                        if (!windowXform.Anchored)
                        {
                            var anchored = _transform.AnchorEntity((windowEntity, windowXform), (ruinGrid.Owner, ruinGrid.Comp), windowPos);
                            isAnchored = anchored && windowXform.Anchored;
                        }
                        else
                        {
                            // Entity is already anchored, verify it's on the correct grid
                            isAnchored = windowXform.GridUid == ruinGrid.Owner;
                            if (!isAnchored)
                            {
                                Log.Warning($"[SalvageSystem] Window {windowProto} at {windowPos} is anchored to wrong grid {windowXform.GridUid}, expected {ruinGrid.Owner}");
                            }
                        }
                    }
                    
                    // If window couldn't be anchored, delete it to prevent stray unanchored windows
                    if (!isAnchored)
                    {
                        Log.Warning($"[SalvageSystem] Failed to anchor window {windowProto} at {windowPos} on grid {ruinGrid.Owner}, deleting it");
                        Del(windowEntity);
                        continue;
                    }
                    
                    // STEP 4: Apply damage to windows (damage simulation applied after spawning)
                    if (windowDamageChance > 0.0f && windowRand.NextSingle() < windowDamageChance)
                    {
                        // Apply moderate damage to window (enough to show damage but not destroy it)
                        if (TryComp<DamageableComponent>(windowEntity, out var damageable))
                        {
                            var damage = new DamageSpecifier();
                            damage.DamageDict["Structural"] = 25; // Moderate structural damage
                            _damageableSystem.TryChangeDamage(windowEntity, damage, damageable: damageable);
                        }
                    }
                }
                
                // Apply biome template for loot spawning (same as debris)
                var biome = EnsureComp<BiomeComponent>(ruinGrid.Owner);
                var biomeTemplateId = new ProtoId<BiomeTemplatePrototype>("SpaceDebris");
                _biome.SetSeed(ruinGrid.Owner, biome, seed);
                _biome.SetTemplate(ruinGrid.Owner, biome, _prototypeManager.Index(biomeTemplateId));
                
                // Mark tiles with walls and windows as modified so biome doesn't spawn on them
                MarkWallAndWindowTilesAsModified(ruinGrid.Owner, biome, ruinResult);
                
                // CRITICAL: Force-load all biome chunks immediately to spawn mobs and loot
                // Biome system normally loads chunks lazily when players are nearby, but ruins are
                // pre-built before players arrive, so we need to force loading
                _biome.ForceLoadAllChunks(ruinGrid.Owner, biome, ruinGrid.Comp);
                
                // Manually spawn additional debris on empty floor tiles
                // The biome system only spawns on completely empty tiles, so we add more variety
                SpawnRuinDebris(ruinGrid, ruinResult, seed);
                
                // Grid is now complete with all tiles, walls, windows, and loot spawners properly placed
                var ruinGrids = new List<(Entity<MapGridComponent> Grid, Box2 Bounds)>
                {
                    (ruinGrid, ruinResult.Bounds)
                };

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        Box2? bounds = null;

        if (salvMapXform.ChildCount == 0)
        {
            Report(magnet.Owner, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
            return;
        }

        var mapChildren = salvMapXform.ChildEnumerator;

        while (mapChildren.MoveNext(out var mapChild))
        {
            // If something went awry in dungen.
            if (!_gridQuery.TryGetComponent(mapChild, out var childGrid))
                continue;

            var childAABB = _transform.GetWorldMatrix(mapChild).TransformBox(childGrid.LocalAABB);
            bounds = bounds?.Union(childAABB) ?? childAABB;

            // Update mass scanner names as relevant.
            if (offering is AsteroidOffering or DebrisOffering)
            {
                _metaData.SetEntityName(mapChild, Loc.GetString("salvage-asteroid-name"));
                _gravity.EnableGravity(mapChild);
            }
        }

        var magnetXform = _xformQuery.GetComponent(magnet.Owner);
        var magnetGridUid = magnetXform.GridUid;
        var attachedBounds = new Box2Rotated();
        var mapId = MapId.Nullspace;
        Angle worldAngle;

        if (magnetGridUid != null)
        {
            var magnetGridXform = _xformQuery.GetComponent(magnetGridUid.Value);
            var (gridPos, gridRot) = _transform.GetWorldPositionRotation(magnetGridXform);
            var gridAABB = _gridQuery.GetComponent(magnetGridUid.Value).LocalAABB;

            attachedBounds = new Box2Rotated(gridAABB.Translated(gridPos), gridRot, gridPos);

            worldAngle = (gridRot + magnetXform.LocalRotation) - MathF.PI / 2;
            mapId = magnetGridXform.MapID;
        }
        else
        {
            worldAngle = _random.NextAngle();
        }

        // Handle ruin placement separately for clustered layout
        if (offering is RuinOffering)
        {
            // Get ruin grids we created (they're already on salvMap from the switch case)
            var ruinGridList = new List<(Entity<MapGridComponent> Grid, Box2 Bounds)>();
            mapChildren = salvMapXform.ChildEnumerator;
            while (mapChildren.MoveNext(out var mapChild))
            {
                if (!_gridQuery.TryGetComponent(mapChild, out var childGrid))
                    continue;

                var childAABB = childGrid.LocalAABB;
                ruinGridList.Add(((mapChild, childGrid), childAABB));
            }

            if (ruinGridList.Count == 0)
            {
                Report(magnet.Owner, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
                _mapSystem.DeleteMap(salvMapXform.MapID);
                return;
            }

            // Plan clustered placements
            if (!PlanRuinPlacements(magnet, mapId, attachedBounds, ruinGridList, worldAngle, ruinConfig, out var placements))
            {
                Report(magnet.Owner, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
                _mapSystem.DeleteMap(salvMapXform.MapID);
                return;
            }

            if (placements.Count == 0 || !_mapSystem.TryGetMap(placements[0].Position.MapId, out var spawnUid))
            {
                _mapSystem.DeleteMap(salvMapXform.MapID);
                return;
            }

            data.Comp.ActiveEntities = null;

            // Place each ruin at its planned position
            for (var i = 0; i < ruinGridList.Count && i < placements.Count; i++)
            {
                var (grid, _) = ruinGridList[i];
                var (placementPos, placementAngle) = placements[i];
                var salvXForm = _xformQuery.GetComponent(grid.Owner);
                var localPos = salvXForm.LocalPosition;

                _transform.SetParent(grid.Owner, salvXForm, spawnUid.Value);
                _transform.SetWorldPositionRotation(grid.Owner, placementPos.Position + localPos, placementAngle, salvXForm);

                data.Comp.ActiveEntities ??= new List<EntityUid>();
                data.Comp.ActiveEntities.Add(grid.Owner);

                // Handle mob restrictions
                var children = salvXForm.ChildEnumerator;
                while (children.MoveNext(out var child))
                {
                    if (!_salvMobQuery.TryGetComponent(child, out var salvMob))
                        continue;

                    salvMob.LinkedEntity = grid.Owner;
                }
            }
        }
        else
        {
            // Standard placement for other offerings
            if (!TryGetSalvagePlacementLocation(magnet, mapId, attachedBounds, bounds!.Value, worldAngle, out var spawnLocation, out var spawnAngle))
            {
                Report(magnet.Owner, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
                _mapSystem.DeleteMap(salvMapXform.MapID);
                return;
            }

            // I have no idea if we want to return on failure or not
            // but I assume trying to set the parent with a null value wouldn't have worked out anyways
            if (!_mapSystem.TryGetMap(spawnLocation.MapId, out var spawnUid))
                return;

            data.Comp.ActiveEntities = null;
            mapChildren = salvMapXform.ChildEnumerator;

            // It worked, move it into position and cleanup values.
            while (mapChildren.MoveNext(out var mapChild))
            {
                var salvXForm = _xformQuery.GetComponent(mapChild);
                var localPos = salvXForm.LocalPosition;

                _transform.SetParent(mapChild, salvXForm, spawnUid.Value);
                _transform.SetWorldPositionRotation(mapChild, spawnLocation.Position + localPos, spawnAngle, salvXForm);

                data.Comp.ActiveEntities ??= new List<EntityUid>();
                data.Comp.ActiveEntities?.Add(mapChild);

                // Handle mob restrictions
                var children = salvXForm.ChildEnumerator;

                while (children.MoveNext(out var child))
                {
                    if (!_salvMobQuery.TryGetComponent(child, out var salvMob))
                        continue;

                    salvMob.LinkedEntity = mapChild;
                }
            }
        }

        Report(magnet.Owner, MagnetChannel, "salvage-system-announcement-arrived", ("timeLeft", data.Comp.ActiveTime.TotalSeconds));
        _mapSystem.DeleteMap(salvMapXform.MapID);

        data.Comp.Announced = false;

        var active = new SalvageMagnetActivatedEvent()
        {
            Magnet = magnet,
        };

        RaiseLocalEvent(ref active);
    }

    private bool PlanRuinPlacements(
        Entity<SalvageMagnetComponent> magnet,
        MapId mapId,
        Box2Rotated attachedBounds,
        List<(Entity<MapGridComponent> Grid, Box2 Bounds)> ruins,
        Angle worldAngle,
        SalvageMagnetRuinConfigPrototype? config,
        out List<(MapCoordinates Position, Angle Rotation)> placements)
    {
        placements = new List<(MapCoordinates, Angle)>();

        if (ruins.Count == 0)
            return false;

        var attachedAABB = attachedBounds.CalcBoundingBox();
        var magnetPos = _transform.GetWorldPosition(magnet);
        var origin = attachedAABB.ClosestPoint(magnetPos);
        
        // Place ruins at configured distance away in the direction the magnet is facing
        var ruinSpawnDistance = config?.RuinSpawnDistance ?? 64f;
        var fraction = 1.0f;

        var placedBounds = new List<Box2Rotated>();
        var clusterCenter = Vector2.Zero;

        for (var i = 0; i < ruins.Count; i++)
        {
            var (_, ruinBounds) = ruins[i];
            var ruinSize = ruinBounds.Size;
            var maxDimension = Math.Max(ruinSize.X, ruinSize.Y);

            Angle rotation = Angle.Zero;
            MapCoordinates position = MapCoordinates.Nullspace;
            bool found = false;

            if (i == 0)
            {
                // First ruin: place at 64 tiles distance with lateral offset
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    var randomPos = origin +
                                    worldAngle.ToVec() * (ruinSpawnDistance * fraction) +
                                    (worldAngle + Math.PI / 2).ToVec() * _random.NextFloat(-magnet.Comp.LateralOffset, magnet.Comp.LateralOffset);
                    position = new MapCoordinates(randomPos, mapId);

                    rotation = _random.NextAngle();
                    var box2 = Box2.CenteredAround(position.Position, ruinSize);
                    var box2Rot = new Box2Rotated(box2, rotation, position.Position);

                    // Check if any grids intersect this position
                    var intersectingGrids = new List<Entity<MapGridComponent>>();
                    if (_mapSystem.TryGetMap(mapId, out var mapEnt))
                        _mapManager.FindGridsIntersecting(mapEnt.Value, box2Rot, ref intersectingGrids);
                    
                    if (intersectingGrids.Count == 0)
                    {
                        found = true;
                        clusterCenter = randomPos;
                        placedBounds.Add(box2Rot);
                        break;
                    }

                    fraction += 0.1f;
                }
            }
            else
            {
                // Subsequent ruins: place in expanding spiral around cluster center
                // Start close to cluster center and expand outward
                var spiralRadius = maxDimension * 1.2f;
                var spiralAngle = _random.NextFloat(0, MathF.PI * 2); // Random starting angle
                var spiralStep = MathF.PI / 3f; // 6 positions per ring

                for (var ring = 1; ring <= 8 && !found; ring++)
                {
                    spiralRadius = maxDimension * (1.2f + ring * 0.4f);

                    for (var step = 0; step < 6 * ring && !found; step++)
                    {
                        var offset = new Vector2(
                            MathF.Cos(spiralAngle) * spiralRadius,
                            MathF.Sin(spiralAngle) * spiralRadius
                        );

                        position = new MapCoordinates(clusterCenter + offset, mapId);
                        rotation = _random.NextAngle();

                        var box2 = Box2.CenteredAround(position.Position, ruinSize);
                        var box2Rot = new Box2Rotated(box2, rotation, position.Position);

                        // Check intersection with existing ruins using bounding boxes
                        var intersects = false;
                        var box2RotAABB = box2Rot.CalcBoundingBox();
                        foreach (var existing in placedBounds)
                        {
                            var existingAABB = existing.CalcBoundingBox();
                            var intersection = box2RotAABB.Intersect(existingAABB);
                            if (intersection.Width > 0 && intersection.Height > 0)
                            {
                                intersects = true;
                                break;
                            }
                        }

                        // Check intersection with other grids
                        if (!intersects)
                        {
                            var checkGrids = new List<Entity<MapGridComponent>>();
                            if (_mapSystem.TryGetMap(mapId, out var checkMapEnt))
                                _mapManager.FindGridsIntersecting(checkMapEnt.Value, box2Rot, ref checkGrids);
                            
                            if (checkGrids.Count == 0)
                            {
                                found = true;
                                placedBounds.Add(box2Rot);
                                break;
                            }
                        }

                        spiralAngle += spiralStep;
                    }
                }
            }

            if (!found)
            {
                // Failed to place this ruin, but continue with others
                continue;
            }

            // Only add if position was successfully found
            if (position != MapCoordinates.Nullspace)
            {
                placements.Add((position, rotation));
            }
        }

        return placements.Count > 0;
    }

    private bool TryGetSalvagePlacementLocation(Entity<SalvageMagnetComponent> magnet, MapId mapId, Box2Rotated attachedBounds, Box2 bounds, Angle worldAngle, out MapCoordinates coords, out Angle angle)
    {
        var attachedAABB = attachedBounds.CalcBoundingBox();
        var magnetPos = _transform.GetWorldPosition(magnet) + worldAngle.ToVec() * bounds.MaxDimension;
        var origin = attachedAABB.ClosestPoint(magnetPos);
        var fraction = 0.50f;

        // Thanks 20kdc
        for (var i = 0; i < 20; i++)
        {
            var randomPos = origin +
                            worldAngle.ToVec() * (magnet.Comp.MagnetSpawnDistance * fraction) +
                            (worldAngle + Math.PI / 2).ToVec() * _random.NextFloat(-magnet.Comp.LateralOffset, magnet.Comp.LateralOffset);
            var finalCoords = new MapCoordinates(randomPos, mapId);

            angle = _random.NextAngle();
            var box2 = Box2.CenteredAround(finalCoords.Position, bounds.Size);
            var box2Rot = new Box2Rotated(box2, angle, finalCoords.Position);

            // This doesn't stop it from spawning on top of random things in space
            // Might be better like this, ghosts could stop it before
            var checkIntersectingGrids = new List<Entity<MapGridComponent>>();
            if (_mapSystem.TryGetMap(finalCoords.MapId, out var checkMapEnt))
                _mapManager.FindGridsIntersecting(checkMapEnt.Value, box2Rot, ref checkIntersectingGrids);
            
            if (checkIntersectingGrids.Count > 0)
            {
                // Bump it further and further just in case.
                fraction += 0.1f;
                continue;
            }

            coords = finalCoords;
            return true;
        }

        angle = Angle.Zero;
        coords = MapCoordinates.Nullspace;
        return false;
    }

    /// <summary>
    /// Marks tiles that have walls or windows as modified in the biome component.
    /// This prevents the biome system from spawning loot on these tiles.
    /// </summary>
    private void MarkWallAndWindowTilesAsModified(EntityUid gridUid, BiomeComponent biome, SalvageRuinGeneratorSystem.RuinResult ruinResult)
    {
        // Collect all tiles that should be marked as modified
        var tilesToMark = new List<Vector2i>();
        
        // Add all wall positions
        foreach (var (wallPos, _) in ruinResult.WallEntities)
        {
            tilesToMark.Add(wallPos);
        }
        
        // Add all window positions
        foreach (var (windowPos, _, _) in ruinResult.WindowEntities)
        {
            tilesToMark.Add(windowPos);
        }
        
        // Use BiomeSystem's public method to mark tiles
        _biome.MarkTilesAsModified(gridUid, biome, tilesToMark);
    }

    /// <summary>
    /// Spawns additional debris entities on empty floor tiles in a ruin.
    /// This supplements the biome system which only spawns on completely empty tiles.
    /// </summary>
    private void SpawnRuinDebris(Entity<MapGridComponent> grid, SalvageRuinGeneratorSystem.RuinResult ruinResult, int seed)
    {
        var debrisRandom = new System.Random(seed);
        
        // Load debris configuration from prototype
        var debrisEntities = new List<(string Proto, float Chance)>();
        var debrisProtoId = new ProtoId<SalvageRuinDebrisPrototype>("Default");
        
        if (_prototypeManager.TryIndex(debrisProtoId, out var debrisProto))
        {
            foreach (var entry in debrisProto.Entries)
            {
                debrisEntities.Add((entry.Proto, entry.Chance));
            }
        }
        else
        {
            // Fallback to hardcoded list if prototype is missing
            Log.Warning("[SalvageSystem] Failed to load SalvageRuinDebris prototype 'Default', using fallback configuration");
            debrisEntities = new List<(string Proto, float Chance)>
            {
                ("Girder", 0.15f),
                ("Grille", 0.10f),
                ("Table", 0.10f),
                ("Rack", 0.12f),
            };
        }
        
        if (debrisEntities.Count == 0)
        {
            Log.Warning("[SalvageSystem] No debris entities configured for ruins");
            return;
        }
        
        // Build hashsets for quick position lookups
        var wallPositions = new HashSet<Vector2i>();
        foreach (var (wallPos, _) in ruinResult.WallEntities)
        {
            wallPositions.Add(wallPos);
        }
        
        var windowPositions = new HashSet<Vector2i>();
        foreach (var (windowPos, _, _) in ruinResult.WindowEntities)
        {
            windowPositions.Add(windowPos);
        }
        
        // Check each floor tile in the ruin
        foreach (var (tilePos, tile) in ruinResult.FloorTiles)
        {
            // Skip tiles designated for walls or windows
            if (wallPositions.Contains(tilePos) || windowPositions.Contains(tilePos))
                continue;
            
            // Check if this tile already has an anchored entity
            var anchored = _mapSystem.GetAnchoredEntitiesEnumerator(grid.Owner, grid.Comp, tilePos);
            if (anchored.MoveNext(out _))
                continue; // Tile already has something, skip
            
            // Randomly decide if we should spawn debris here
            if (debrisRandom.NextDouble() > 0.25f) // 25% chance to spawn something
                continue;
            
            // Pick a random debris entity
            var roll = debrisRandom.NextSingle();
            var cumulativeChance = 0f;
            
            foreach (var (proto, chance) in debrisEntities)
            {
                cumulativeChance += chance;
                if (roll <= cumulativeChance)
                {
                    // Spawn the debris
                    var coords = new EntityCoordinates(grid.Owner, tilePos);
                    var debris = SpawnAtPosition(proto, coords);
                    
                    // Try to anchor it
                    var xform = Transform(debris);
                    if (xform != null && !xform.Anchored)
                    {
                        _transform.AnchorEntity((debris, xform), (grid.Owner, grid.Comp), tilePos);
                    }
                    
                    break;
                }
            }
        }
    }
}

[ByRefEvent]
public record struct SalvageMagnetActivatedEvent
{
    public EntityUid Magnet;
}
