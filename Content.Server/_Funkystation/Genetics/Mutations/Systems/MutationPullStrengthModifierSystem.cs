using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationPullStrengthModifierSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationPullStrengthModifierComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid, MutationPullStrengthModifierComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<PullerComponent>(uid, out var puller) || puller.Pulling == null)
            return;

        args.ModifySpeed(args.WalkSpeedModifier * comp.PullSlowdownMultiplier,
                        args.SprintSpeedModifier * comp.PullSlowdownMultiplier);
    }
}
