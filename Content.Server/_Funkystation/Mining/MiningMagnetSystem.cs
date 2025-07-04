// Server.SalvageSystem.Magnet used as a base with necessary things plucked from Server.SalvageSystem
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Content.Shared.Mobs.Components;
using Content.Shared.Procedural;
using Content.Shared.Radio;
using Content.Shared._Funkystation.Mining;
using Robust.Shared.Exceptions;
using Robust.Shared.Map;

// Prolly a lot of redundant/unneeded namespaces but I'd rather have them and not need them
using Content.Server.Radio.EntitySystems;
using Content.Shared.Radio;
using Content.Shared._Funkystation.Mining;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Chat.Managers;
using Content.Server.Gravity;
using Content.Server.Parallax;
using Content.Server.Procedural;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Construction.EntitySystems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Content.Server.Labels;
using Robust.Shared.EntitySerialization.Systems;

namespace Content.Server._Funkystation.Mining;

public sealed partial class MiningMagnetSystem : SharedMiningSystem
{
    [Dependency] private readonly IRuntimeLog _runtimeLog = default!;
    // Ditto here from namespace comment
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AnchorableSystem _anchorable = default!;
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly DungeonSystem _dungeon = default!;
    [Dependency] private readonly GravitySystem _gravity = default!;
    [Dependency] private readonly LabelSystem _labelSystem = default!;
    [Dependency] private readonly MapLoaderSystem _loader = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly RadioSystem _radioSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConsoles = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    [ValidatePrototypeId<RadioChannelPrototype>]
    private const string MagnetChannel = "Supply";

    private EntityQuery<MiningMobRestrictionsComponent> _salvMobQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    private List<(Entity<TransformComponent> Entity, EntityUid MapUid, Vector2 LocalPosition)> _detachEnts = new();

    // necessary vars from Server.SalvageSystem
    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    // Fusion of Initialize() from SalvageSystem and InitializeMagnet() from SalvageSystem.Magnet
    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();

        _salvMobQuery = GetEntityQuery<MiningMobRestrictionsComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<MiningMagnetDataComponent, MapInitEvent>(OnMagnetDataMapInit);

        SubscribeLocalEvent<MiningMagnetTargetComponent, GridSplitEvent>(OnMagnetTargetSplit);

        SubscribeLocalEvent<MiningMagnetComponent, MagnetClaimOfferEvent>(OnMagnetClaim);
        SubscribeLocalEvent<MiningMagnetComponent, ComponentStartup>(OnMagnetStartup);
        SubscribeLocalEvent<MiningMagnetComponent, AnchorStateChangedEvent>(OnMagnetAnchored);
    }

    // from SalvageSystem, needed for radio announcements
    private void Report(EntityUid source, string channelName, string messageKey, params (string, object)[] args)
    {
        var message = args.Length == 0 ? Loc.GetString(messageKey) : Loc.GetString(messageKey, args);
        var channel = _prototypeManager.Index<RadioChannelPrototype>(channelName);
        _radioSystem.SendRadioMessage(source, message, channel, source);
    }

    private void OnMagnetClaim(EntityUid uid, MiningMagnetComponent component, ref MagnetClaimOfferEvent args)
    {
        var station = _station.GetOwningStation(uid);

        if (!TryComp(station, out MiningMagnetDataComponent? dataComp) ||
            dataComp.EndTime != null)
        {
            return;
        }

        TakeMagnetOffer((station.Value, dataComp), args.Index, (uid, component));
    }

    private void OnMagnetStartup(EntityUid uid, MiningMagnetComponent component, ComponentStartup args)
    {
        UpdateMagnetUI((uid, component), Transform(uid));
    }

    private void OnMagnetAnchored(EntityUid uid, MiningMagnetComponent component, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        UpdateMagnetUI((uid, component), args.Transform);
    }

    private void OnMagnetDataMapInit(EntityUid uid, MiningMagnetDataComponent component, ref MapInitEvent args)
    {
        CreateMagnetOffers((uid, component));
    }

    private void OnMagnetTargetSplit(EntityUid uid, MiningMagnetTargetComponent component, ref GridSplitEvent args)
    {
        // Don't think I'm not onto you people splitting to make new grids.
        if (TryComp(component.DataTarget, out MiningMagnetDataComponent? dataComp))
        {
            foreach (var gridUid in args.NewGrids)
            {
                dataComp.ActiveEntities?.Add(gridUid);
            }
        }
    }

    // Fusion of Update() from SalvageSystem and UpdateMagnet() from SalvageSystem.Magnet
    public override void Update(float frameTime)
    {
        var dataQuery = EntityQueryEnumerator<MiningMagnetDataComponent>();
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
                            "mining-system-announcement-losing",
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
    private void EndMagnet(Entity<MiningMagnetDataComponent> data)
    {
        if (data.Comp.ActiveEntities != null)
        {
            // Handle mobrestrictions getting deleted
            var query = AllEntityQuery<MiningMobRestrictionsComponent>();

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

    private void CreateMagnetOffers(Entity<MiningMagnetDataComponent> data)
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
    private Entity<MiningMagnetComponent>? GetMagnet(Entity<MiningMagnetDataComponent> data)
    {
        var query = AllEntityQuery<MiningMagnetComponent, TransformComponent>();

        while (query.MoveNext(out var magnetUid, out var magnet, out var xform))
        {
            var stationUid = _station.GetOwningStation(magnetUid, xform);

            if (stationUid != data.Owner)
                continue;

            return (magnetUid, magnet);
        }

        return null;
    }

    private void UpdateMagnetUI(Entity<MiningMagnetComponent> entity, TransformComponent xform)
    {
        var station = _station.GetOwningStation(entity, xform);

        if (!TryComp(station, out MiningMagnetDataComponent? dataComp))
            return;

        _ui.SetUiState(entity.Owner, MiningMagnetUiKey.Key,
            new MiningMagnetBoundUserInterfaceState(dataComp.Offered)
            {
                Cooldown = dataComp.OfferCooldown,
                Duration = dataComp.ActiveTime,
                EndTime = dataComp.EndTime,
                NextOffer = dataComp.NextOffer,
                ActiveSeed = dataComp.ActiveSeed,
            });
    }

    private void UpdateMagnetUIs(Entity<MiningMagnetDataComponent> data)
    {
        var query = AllEntityQuery<MiningMagnetComponent, TransformComponent>();

        while (query.MoveNext(out var magnetUid, out var magnet, out var xform))
        {
            var station = _station.GetOwningStation(magnetUid, xform);

            if (station != data.Owner)
                continue;

            _ui.SetUiState(magnetUid, MiningMagnetUiKey.Key,
                new MiningMagnetBoundUserInterfaceState(data.Comp.Offered)
                {
                    Cooldown = data.Comp.OfferCooldown,
                    Duration = data.Comp.ActiveTime,
                    EndTime = data.Comp.EndTime,
                    NextOffer = data.Comp.NextOffer,
                    ActiveSeed = data.Comp.ActiveSeed,
                });
        }
    }

    private async Task TakeMagnetOffer(Entity<MiningMagnetDataComponent> data, int index, Entity<MiningMagnetComponent> magnet)
    {
        var seed = data.Comp.Offered[index];

        var offering = GetMiningOffering(seed);
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
            default:
                throw new ArgumentOutOfRangeException();
        }

        Box2? bounds = null;

        if (salvMapXform.ChildCount == 0)
        {
            Report(magnet.Owner, MagnetChannel, "mining-system-announcement-spawn-no-debris-available");
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
            if (offering is AsteroidOffering)
            {
                _metaData.SetEntityName(mapChild, Loc.GetString("mining-asteroid-name"));
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

        if (!TryGetSalvagePlacementLocation(magnet, mapId, attachedBounds, bounds!.Value, worldAngle, out var spawnLocation, out var spawnAngle))
        {
            Report(magnet.Owner, MagnetChannel, "magnet-system-announcement-spawn-no-debris-available");
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

        Report(magnet.Owner, MagnetChannel, "mining-system-announcement-arrived", ("timeLeft", data.Comp.ActiveTime.TotalSeconds));
        _mapSystem.DeleteMap(salvMapXform.MapID);

        data.Comp.Announced = false;

        var active = new MiningMagnetActivatedEvent()
        {
            Magnet = magnet,
        };

        RaiseLocalEvent(ref active);
    }


    private bool TryGetSalvagePlacementLocation(Entity<MiningMagnetComponent> magnet, MapId mapId, Box2Rotated attachedBounds, Box2 bounds, Angle worldAngle, out MapCoordinates coords, out Angle angle)
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
public record struct MiningMagnetActivatedEvent
{
    public EntityUid Magnet;
}
