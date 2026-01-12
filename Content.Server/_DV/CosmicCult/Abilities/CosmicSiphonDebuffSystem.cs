//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._DV.CosmicCult.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Medical;
using Content.Shared.Bed.Sleep;
using Content.Shared.IdentityManagement;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.CosmicCult.Abilities;

public sealed class CosmicSiphonDebuffSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    private const string SleepKey = "ForcedSleep"; // Same as Narcolepsy & others.
    private const string MigraineKey = "Migraine"; // used for generic migraines
    private readonly SoundSpecifier _teleportSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_lapse.ogg");
    private readonly EntProtoId _teleportInVFX = "CosmicLapseAbilityVFX";
    private readonly EntProtoId _teleportOutVFX = "CosmicGenericVFX";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CosmicDebuffQueueComponent, ComponentInit>(OnComponentInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CosmicDebuffQueueComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime > comp.ActivationTime)
            {
                ActivateCosmicDebuff(uid, comp.SelectedDebuff);
                comp.DebuffQuant--; // decrement debuffs in queue
                if (comp.DebuffQuant > 0)
                {
                    comp.ActivationTime = _timing.CurTime + _random.Next(comp.MinTimeInQueue, comp.MaxTimeInQueue);
                    comp.SelectedDebuff = _random.Pick(comp.DebuffOptions);
                }
                else
                    RemComp<CosmicDebuffQueueComponent>(uid);
            }
        }
    }

    private void OnComponentInit(Entity<CosmicDebuffQueueComponent> ent, ref ComponentInit args)
    {
        ent.Comp.ActivationTime = _timing.CurTime + _random.Next(ent.Comp.MinTimeInQueue, ent.Comp.MaxTimeInQueue);
        ent.Comp.SelectedDebuff = _random.Pick(ent.Comp.DebuffOptions);
    }

    private void ActivateCosmicDebuff(EntityUid ent, Enum debuff)
    {
        switch (debuff)
        {
            case CosmicDebuffOptions.CosmicDebuffMigraine:
                _statusEffects.TryAddStatusEffect<MigraineComponent>(ent, MigraineKey, TimeSpan.FromSeconds(_random.Next(15, 55)), false);
                break;
            case CosmicDebuffOptions.CosmicDebuffStutter:
                _stutter.DoStutter(ent, TimeSpan.FromSeconds(_random.Next(25, 45)), true);
                break;
            case CosmicDebuffOptions.CosmicDebuffVomiting:
                _vomit.Vomit(ent);
                break;
            case CosmicDebuffOptions.CosmicDebuffSleeping:
                _statusEffects.TryAddStatusEffect<ForcedSleepingComponent>(ent, SleepKey, TimeSpan.FromSeconds(_random.Next(5, 15)), true);
                break;
            case CosmicDebuffOptions.CosmicDebuffTeleporting:
                _stun.TryStun(ent, TimeSpan.FromSeconds(_random.Next(2, 6)), true);
                TeleportBadLuck(ent);
                _vomit.Vomit(ent);
                break;
            default:
                break;
        }
    }

    private void TeleportBadLuck(EntityUid ent)
    {
        var xform = Transform(ent);
        if (xform.GridUid is null)
            return;

        var grid = xform.GridUid.Value;

        if (!TryComp<MapGridComponent>(grid, out var gridComp))
            return;

        var targetCoords = xform.Coordinates;
        var gridBounds = gridComp.LocalAABB.Scale(0.8f);

        for (var i = 0; i < 25; i++)
        {
            var randomX = _random.Next((int) gridBounds.Left, (int) gridBounds.Right);
            var randomY = _random.Next((int) gridBounds.Bottom, (int) gridBounds.Top);

            var tile = new Vector2i(randomX, randomY);

            // no air-blocked areas.
            if (_atmosphere.IsTileSpace(grid, xform.MapUid, tile) ||
                _atmosphere.IsTileAirBlocked(grid, tile, mapGridComp: gridComp))
            {
                continue;
            }

            // don't spawn inside of solid objects
            var physQuery = GetEntityQuery<PhysicsComponent>();
            var valid = true;

            // TODO: This should be using static lookup.
            foreach (var entity in _mapSystem.GetAnchoredEntities(grid, gridComp, tile))
            {
                if (!physQuery.TryGetComponent(entity, out var body))
                    continue;
                if (body.BodyType != BodyType.Static ||
                    !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }
            if (!valid)
                continue;

            var pos = _mapSystem.GridTileToLocal(grid, gridComp, tile);

            targetCoords = pos;
            break;
        }
        var teleportInEnt = Spawn(_teleportInVFX, xform.Coordinates);
        var teleportOutEnt = Spawn(_teleportOutVFX, targetCoords);
        _audio.PlayPvs(_teleportSFX, teleportInEnt);
        _audio.PlayPvs(_teleportSFX, teleportOutEnt);
        _transform.SetCoordinates(ent, Transform(teleportOutEnt).Coordinates);
    }
}
