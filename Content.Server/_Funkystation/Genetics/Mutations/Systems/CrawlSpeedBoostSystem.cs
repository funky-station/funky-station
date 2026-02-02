using Content.Shared._Funkystation.Genetics.Mutations.Components;
using Content.Shared._Funkystation.Genetics.Mutations.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared._White.Standing;
using Content.Shared.Standing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class CrawlSpeedBoostSystem : SharedCrawlSpeedBoostSystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movespeed = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CrawlSpeedBoostComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
        SubscribeLocalEvent<CrawlSpeedBoostComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, CrawlSpeedBoostComponent comp, ComponentInit args)
    {
        _movespeed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefresh(EntityUid uid, CrawlSpeedBoostComponent comp, RefreshMovementSpeedModifiersEvent args)
    {
        if (!TryComp<LayingDownComponent>(uid, out var laying) ||
            !TryComp<StandingStateComponent>(uid, out var standing) ||
            standing.CurrentState != StandingState.Lying)
            return;

        float original = laying.SpeedModify;
        float boost = comp.TargetSpeedMult / original;

        args.ModifySpeed(boost, boost);
    }
}
