using Content.Server.Actions;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Store.Systems;
using Content.Shared.Revolutionary;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server.Revolutionary;


public sealed partial class HRevSystem : EntitySystem
{
    [Dependency]
    private readonly ActionsSystem _actions = default!;
    [Dependency]
    private readonly MindSystem _mind = default!;
    [Dependency]
    private readonly StoreSystem _store = default!;

    private readonly RevolutionaryRuleComponent _rule = default!;
    public readonly ProtoId<CurrencyPrototype> Currency = "RevCoin";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HRevComponent, ComponentInit>(HRevComponentInitialize);

        SubscribeEvents();
    }

    private void HRevComponentInitialize(EntityUid uid, HRevComponent comp, ref ComponentInit ev)
    {
        if (!_mind.TryGetMind(uid, out _, out _))
        {
            return;
        }

        var storeComp = EnsureComp<StoreComponent>(uid);
        storeComp.CurrencyWhitelist.Add(Currency);

        _actions.AddAction(uid, "ActionHRevOpenStore");
    }
}

