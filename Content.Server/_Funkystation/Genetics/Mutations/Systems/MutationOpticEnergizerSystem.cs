using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared.Actions;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationOpticEnergizerSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationOpticEnergizerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationOpticEnergizerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutationOpticEnergizerComponent, OpticBlastActionEvent>(OnBlast);
    }

    private void OnInit(EntityUid uid, MutationOpticEnergizerComponent comp, ComponentInit args)
    {
        _actions.AddAction(uid, ref comp.GrantedAction, comp.ActionId);
    }

    private void OnShutdown(EntityUid uid, MutationOpticEnergizerComponent comp, ComponentShutdown args)
    {
        if (comp.GrantedAction is { Valid: true } action)
            _actions.RemoveAction(action);
    }

    private void OnBlast(EntityUid uid, MutationOpticEnergizerComponent comp, OpticBlastActionEvent args)
    {
        if (args.Handled || args.Performer != uid)
            return;

        args.Handled = true;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        var from = xform.MapPosition;
        var to = args.Target.Position;

        var direction = (to - from.Position);

        if (direction.LengthSquared() <= 0)
            return;

        if (comp.GrantedAction is { Valid: true } action)
        {
            _audio.PlayPvs(EntityManager.GetComponentOrNull<WorldTargetActionComponent>(action)?.Sound, uid);
        }

        var hitscanProto = _proto.Index<HitscanPrototype>("RedMediumLaser");

        var ammoList = new List<(EntityUid? Entity, IShootable Shootable)>
        {
            (null, hitscanProto)
        };

        var fromCoords = xform.Coordinates;

        _gun.Shoot(uid, new GunComponent(), ammoList, fromCoords, args.Target, out _, uid);
    }
}
