using Content.Server.Actions;
using Content.Server.Mind;
using Content.Server.Store.Systems;
using Content.Shared.Revolutionary;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;
using static Content.Shared.Revolutionary.HeadRevolutionaryPathComponent;

namespace Content.Server.Revolutionary;

public sealed partial class HeadRevolutionarySystem : EntitySystem
{
    [Dependency]
    private readonly ActionsSystem _actions = default!;
    [Dependency]
    private readonly MindSystem _mind = default!;
    [Dependency]
    private readonly StoreSystem _store = default!;

    public readonly ProtoId<CurrencyPrototype> Currency = "RevCoin";

    public readonly Dictionary<RevolutionaryPaths, ProtoId<StoreCategoryPrototype>> RevCoinStore = new()
    {
        {
            RevolutionaryPaths.NONE,
            "RevStoreGeneral"
        },
        {
            RevolutionaryPaths.VANGUARD,
            "RevStoreVanguard"
        },
        {
            RevolutionaryPaths.WARLORD,
            "RevStoreWarlord"
        },
        {
            RevolutionaryPaths.WOTP,
            "RevStoreWotp"
        },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadRevolutionaryPathComponent, ComponentInit>(HeadRevolutionaryPathComponentInitialize);

        SubscribeEvents();
    }

    private void HeadRevolutionaryPathComponentInitialize(EntityUid uid, HeadRevolutionaryPathComponent comp, ref ComponentInit ev)
    {
        if (!_mind.TryGetMind(uid, out _, out _))
        {
            return;
        }

        var storeComp = EnsureComp<StoreComponent>(uid);
        storeComp.CurrencyWhitelist.Add(Currency);
        storeComp.Categories.Add(RevCoinStore[RevolutionaryPaths.NONE]);
        storeComp.Balance.Add(Currency, 5);

        _actions.AddAction(uid, "ActionHeadRevolutionaryUplink");
    }
}

