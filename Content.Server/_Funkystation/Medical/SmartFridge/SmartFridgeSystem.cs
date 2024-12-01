using Content.Server.Power.EntitySystems;
using Content.Shared._Funkystation.Medical.SmartFridge;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Server.Audio;

namespace Content.Server._Funkystation.Medical.SmartFridge;

public sealed class SmartFridgeSystem : SharedSmartFridgeSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly TagSystem _tags = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.BuiEvents<SmartFridgeComponent>(SmartFridgeUiKey.Key,
            subs =>
        {
            subs.Event<SmartFridgeEjectMessage>(OnSmartFridgeEjectMessage);
        });

        SubscribeLocalEvent<SmartFridgeComponent, MapInitEvent>(MapInit, before: [typeof(ItemSlotsSystem)]);
        SubscribeLocalEvent<SmartFridgeComponent, ItemSlotEjectAttemptEvent>(OnItemEjectEvent);
        SubscribeLocalEvent<SmartFridgeComponent, InteractUsingEvent>(OnInteractEvent);
    }

    private void OnInteractEvent(EntityUid entity, SmartFridgeComponent component, ref InteractUsingEvent ev)
    {
        if (component.StorageWhitelist != null)
        {
            if (!_tags.HasAnyTag(ev.Used, component.StorageWhitelist.Tags!.ToArray()))
            {
                ev.Handled = true;
                return;
            }
        }

        if (!_itemSlotsSystem.TryInsertEmpty(ev.Target, ev.Used, ev.User, true))
            return;

        component.Inventory = GetInventory(entity);

        ev.Handled = true;

        Dirty(entity, component);
    }

    private void OnItemEjectEvent(EntityUid entity, SmartFridgeComponent component, ref ItemSlotEjectAttemptEvent ev)
    {
        if (component.SlotToEjectFrom == ev.Slot)
        {
            Dirty(entity, component);
            return;
        }

        ev.Cancelled = !component.Ejecting;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SmartFridgeComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Ejecting)
                continue;

            comp.EjectAccumulator += frameTime;
            if (!(comp.EjectAccumulator >= comp.EjectDelay))
                continue;

            comp.EjectAccumulator = 0f;
            comp.Ejecting = false;

            EjectItem(uid, comp);
        }
    }

    private void MapInit(EntityUid uid, SmartFridgeComponent component, MapInitEvent _)
    {
        SetupSmartFridge(uid, component);
    }

    private void OnSmartFridgeEjectMessage(EntityUid uid, SmartFridgeComponent component, SmartFridgeEjectMessage args)
    {
        if (!this.IsPowered(uid, EntityManager))
            return;

        if (args.Actor is not { Valid: true } entity || Deleted(entity))
            return;

        VendFromSlot(uid, args.Id);
        Dirty(uid, component);
    }

    private void VendFromSlot(EntityUid uid, string itemSlotToEject, SmartFridgeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!this.IsPowered(uid, EntityManager))
        {
            return;
        }

        var item = _itemSlotsSystem.GetItemOrNull(uid, itemSlotToEject);

        if (item == null)
            return;

        if (!_itemSlotsSystem.TryGetSlot(uid, itemSlotToEject, out var itemSlot) && itemSlot == null)
            return;

        component.Ejecting = true;
        component.SlotToEjectFrom = itemSlot;

        _audio.PlayPvs(component.SoundVend, uid);
    }

    private void EjectItem(EntityUid uid, SmartFridgeComponent component)
    {
        if (component.SlotToEjectFrom == null ||
            !_itemSlotsSystem.TryEject(uid, component.SlotToEjectFrom, null, out _))
            return;

        component.Inventory = GetInventory(uid);
        component.SlotToEjectFrom = null;

        Dirty(uid, component);
    }

    private void SetupSmartFridge(EntityUid uid, SmartFridgeComponent component)
    {
        //sets up stuff for init
        for (var i = 0; i < component.NumSlots; i++)
        {
            var storageSlotId = SmartFridgeComponent.BaseStorageSlotId + i;
            ItemSlot storageComponent = new()
            {
                Whitelist = component.StorageWhitelist,
                Swap = false,
                EjectOnBreak = true,
            };

            component.StorageSlotIds.Add(storageSlotId);
            component.StorageSlots.Add(storageComponent);
            component.StorageSlots[i].Name = "Storage Slot " + (i+1);
            _itemSlotsSystem.AddItemSlot(uid, component.StorageSlotIds[i], component.StorageSlots[i]);
        }

        _itemSlotsSystem.AddItemSlot(uid, "itemSlot", component.FridgeSlots);
    }
}
