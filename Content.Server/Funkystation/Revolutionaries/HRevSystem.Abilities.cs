
using Content.Shared.Revolutionary;
using Content.Shared.Store.Components;

namespace Content.Server.Revolutionary;

public sealed partial class HRevSystem : EntitySystem {

    public void SubscribeEvents()
    {
        SubscribeLocalEvent<HRevComponent, EventHrevOpenStore>(OnOpenStore);
    }

    private void OnOpenStore(EntityUid uid, HRevComponent comp, ref EventHrevOpenStore args)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.ToggleUi(uid, uid, store);
    }
}
