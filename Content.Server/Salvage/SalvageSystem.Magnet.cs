// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Server.Salvage.Magnet;
using Content.Shared.Mobs.Components;
using Content.Shared.Procedural;
using Content.Shared.Radio;
using Content.Shared.Salvage.Magnet;
using Robust.Shared.Exceptions;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server.Salvage;

public sealed partial class SalvageSystem
{
    [Dependency] private readonly IRuntimeLog _runtimeLog = default!;

    [ValidatePrototypeId<RadioChannelPrototype>]
    private const string MagnetChannel = "Supply";

    private SalvageRuinGenerator? _ruinGenerator;

    private EntityQuery<SalvageMobRestrictionsComponent> _salvMobQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    private List<(Entity<TransformComponent> Entity, EntityUid MapUid, Vector2 LocalPosition)> _detachEnts = new();

    private void InitializeMagnet()
    {
        _salvMobQuery = GetEntityQuery<SalvageMobRestrictionsComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        // Create ruin generator with dependencies
        _ruinGenerator = new SalvageRuinGenerator(
            _mapManager,
            _prototypeManager,
            _random,
            _mapSystem,
            _tileDefinitionManager,
            _loader);

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

        TakeMagnetOffer((station.Value, dataComp), args.Index, (uid, component));
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
                // Generate 5 ruins
                const int ruinCount = 5;
                var ruinResults = new List<SalvageRuinGenerator.RuinResult>();

                for (var i = 0; i < ruinCount; i++)
                {
                    var ruinSeed = seed + i;
                    var ruinResult = _ruinGenerator?.GenerateRuin(ruin.RuinMap.MapPath, ruinSeed, floodFillPoints: 50);
                    if (ruinResult != null)
                    {
                        ruinResults.Add(ruinResult);
                    }
                }

                if (ruinResults.Count == 0)
                {
                    Report(magnet, MagnetChannel, "salvage-system-announcement-spawn-no-debris-available");
                    _mapSystem.DeleteMap(salvMapXform.MapID);
                    return;
                }

                // Create grids for each ruin
                var ruinGrids = new List<(Entity<MapGridComponent> Grid, Box2 Bounds)>();
                foreach (var ruinResult in ruinResults)
                {
                    var ruinGrid = _mapManager.CreateGridEntity(salvMap);
                    _mapSystem.SetTiles(ruinGrid.Owner, ruinGrid.Comp, ruinResult.FloorTiles);
                    ruinGrids.Add((ruinGrid, ruinResult.Bounds));
                }

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
            if (!PlanRuinPlacements(magnet, mapId, attachedBounds, ruinGridList, worldAngle, out var placements))
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
        out List<(MapCoordinates Position, Angle Rotation)> placements)
    {
        placements = new List<(MapCoordinates, Angle)>();

        if (ruins.Count == 0)
            return false;

        var attachedAABB = attachedBounds.CalcBoundingBox();
        var magnetPos = _transform.GetWorldPosition(magnet) + worldAngle.ToVec() * 32f; // Initial offset
        var origin = attachedAABB.ClosestPoint(magnetPos);
        var fraction = 0.50f;

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
                // First ruin: place at standard distance
                for (var attempt = 0; attempt < 20; attempt++)
                {
                    var randomPos = origin +
                                    worldAngle.ToVec() * (magnet.Comp.MagnetSpawnDistance * fraction) +
                                    (worldAngle + Math.PI / 2).ToVec() * _random.NextFloat(-magnet.Comp.LateralOffset, magnet.Comp.LateralOffset);
                    position = new MapCoordinates(randomPos, mapId);

                    rotation = _random.NextAngle();
                    var box2 = Box2.CenteredAround(position.Position, ruinSize);
                    var box2Rot = new Box2Rotated(box2, rotation, position.Position);

                    if (!_mapManager.FindGridsIntersecting(mapId, box2Rot).Any())
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
                var spiralRadius = maxDimension * 1.5f;
                var spiralAngle = 0f;
                var spiralStep = MathF.PI / 4f; // 8 positions per ring

                for (var ring = 1; ring <= 10 && !found; ring++)
                {
                    spiralRadius = maxDimension * (1.5f + ring * 0.5f);

                    for (var step = 0; step < 8 * ring && !found; step++)
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
                        if (!intersects && !_mapManager.FindGridsIntersecting(mapId, box2Rot).Any())
                        {
                            found = true;
                            placedBounds.Add(box2Rot);
                            break;
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
            if (_mapManager.FindGridsIntersecting(finalCoords.MapId, box2Rot).Any())
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
}

[ByRefEvent]
public record struct SalvageMagnetActivatedEvent
{
    public EntityUid Magnet;
}
