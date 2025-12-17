using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

[Access(typeof(MovementSpeedModifierSystem))]
public sealed class MutationSpeedBoostSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _moveSpeedSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationSpeedBoostComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationSpeedBoostComponent, ComponentRemove>(OnRemove);
        SubscribeLocalEvent<MutationSpeedBoostComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovement);
    }

    private void OnInit(EntityUid uid, MutationSpeedBoostComponent ent, ComponentInit args)
    {
        _moveSpeedSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRemove(EntityUid uid, MutationSpeedBoostComponent ent, ComponentRemove args)
    {
        _moveSpeedSystem.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMovement(Entity<MutationSpeedBoostComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.WalkMultiplier, ent.Comp.SprintMultiplier);
    }
}
