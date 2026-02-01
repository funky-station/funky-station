using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Actions;
using Content.Shared.Magic.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationCryokinesisSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationCryokinesisComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationCryokinesisComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutationCryokinesisComponent, ProjectileSpellEvent>(OnFireball);
    }

    private void OnInit(EntityUid uid, MutationCryokinesisComponent comp, ComponentInit args)
    {
        _actions.AddAction(uid, ref comp.GrantedAction, "ActionGeneticIceball");
    }

    private void OnShutdown(EntityUid uid, MutationCryokinesisComponent comp, ComponentShutdown args)
    {
        if (comp.GrantedAction is { Valid: true } action)
            _actions.RemoveAction(action);
    }

    private void OnFireball(EntityUid uid, MutationCryokinesisComponent comp, ProjectileSpellEvent args)
    {
        if (args.Handled) return;

        var curTime = _timing.CurTime;
        if (curTime < comp.NextUse) return;

        if (!TryComp<TransformComponent>(uid, out var xform)) return;

        var fromCoords = xform.Coordinates;
        var toCoords = args.Target;

        var fromMap = fromCoords.ToMap(EntityManager, _transform);
        var spawnCoords = _mapManager.TryFindGridAt(fromMap, out var gridUid, out _)
            ? fromCoords.WithEntityId(gridUid, EntityManager)
            : new EntityCoordinates(_mapManager.GetMapEntityId(fromMap.MapId), fromMap.Position);

        var fireball = Spawn(args.Prototype, spawnCoords);

        var direction = toCoords.ToMapPos(EntityManager, _transform) -
                        spawnCoords.ToMapPos(EntityManager, _transform);

        var userVelocity = _physics.GetMapLinearVelocity(uid);

        _gun.ShootProjectile(fireball, direction, userVelocity, uid, uid);

        if (comp.GrantedAction is { Valid: true } action)
        {
            _audio.PlayPvs(EntityManager.GetComponentOrNull<WorldTargetActionComponent>(action)?.Sound, uid);
        }

        comp.NextUse = curTime + TimeSpan.FromSeconds(comp.Cooldown);
        args.Handled = true;
    }
}
