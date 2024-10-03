
using Content.Shared.Actions;
using Content.Shared.Revolutionary;
using Content.Shared.Store.Components;

namespace Content.Server.Revolutionary;

public sealed partial class HRevSystem : EntitySystem {

    public void SubscribeEvents()
    {
        SubscribeLocalEvent<HRevComponent, EventHRevOpenStore>(OnOpenStore);
        SubscribeLocalEvent<HRevComponent, HRevSelectedVanguardEvent>(OnSelectVanguardPath);
        SubscribeLocalEvent<HRevComponent, HRevSelectedWarlordEvent>(OnSelectWarlordPath);
        SubscribeLocalEvent<HRevComponent, HRevSelectedWOTPEvent>(OnSelectWOTPPath);
    }

    private void OnOpenStore(EntityUid uid, HRevComponent comp, ref EventHRevOpenStore args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }

    private void OnSelectVanguardPath(EntityUid uid, HRevComponent comp, ref HRevSelectedVanguardEvent ev)
    {
        comp.CurrentPath = HRevComponent.RevolutionaryPaths.VANGUARD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[HRevComponent.RevolutionaryPaths.VANGUARD]);
    }

    private void OnSelectWarlordPath(EntityUid uid, HRevComponent comp, ref HRevSelectedWarlordEvent ev)
    {
        comp.CurrentPath = HRevComponent.RevolutionaryPaths.WARLORD;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[HRevComponent.RevolutionaryPaths.WARLORD]);
    }

    private void OnSelectWOTPPath(EntityUid uid, HRevComponent comp, ref HRevSelectedWOTPEvent ev)
    {
        comp.CurrentPath = HRevComponent.RevolutionaryPaths.WOTP;

        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        store.Categories.Add(RevCoinStore[HRevComponent.RevolutionaryPaths.WOTP]);
    }
}
