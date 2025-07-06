using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Traits.Assorted;

namespace Content.Server.StationEvents.Events;

public sealed class FaultyChipsRule : StationEventSystem<FaultyChipsRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(PlayerSpawned);
    }

    protected override void Added(EntityUid uid,
        FaultyChipsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;

        base.Added(uid, component, gameRule, args);
    }


    protected override void Started(EntityUid uid,
        FaultyChipsRuleComponent component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var ent, out _))
        {

            if (!EnsureComp<UnrevivableComponent>(ent, out var unrevivable))
            {
                unrevivable.Analyzable = false;
                unrevivable.ReasonMessage = "defib-faulty-chip";
            }

        }
    }

    private void PlayerSpawned(PlayerSpawnCompleteEvent args)
    {
        if (!EnsureComp<UnrevivableComponent>(args.Mob, out var unrevivable))
        {
            unrevivable.Analyzable = false;
            unrevivable.ReasonMessage = "defib-faulty-chip";
        }
    }

}
