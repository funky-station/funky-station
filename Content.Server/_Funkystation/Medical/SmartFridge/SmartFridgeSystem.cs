using System.Linq;
using Content.Server.Power.EntitySystems;
using Content.Shared._Funkystation.Medical.SmartFridge;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Interaction;
using Content.Shared.Tag;
using Robust.Server.Audio;

namespace Content.Server._Funkystation.Medical.SmartFridge;

public sealed class SmartFridgeSystem : SharedSmartFridgeSystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly AnchorableSystem _anchorable = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
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
        // SubscribeLocalEvent<SmartFridgeComponent, ItemSlotEjectAttemptEvent>(OnItemEjectEvent);
        SubscribeLocalEvent<SmartFridgeComponent, InteractUsingEvent>(OnInteractEvent);
    }

    private void OnInteractEvent(EntityUid entity, SmartFridgeComponent component, ref InteractUsingEvent ev)
    {
        if (_tags.HasTag(ev.Used, "Wrench"))
        {
            _anchorable.TryToggleAnchor(entity, ev.User, ev.Used);

            ev.Handled = true;
        }

        if (!_anchorable.IsPowered(entity, _entityManager))
        {
            ev.Handled = true;
            return;
        }

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

    // renabling this will prevent one from dispensing
    // this would probably have to be added to the foreach loop for it to work
    // but i dont need it rn so. Delete
    private void OnItemEjectEvent(EntityUid entity, SmartFridgeComponent component, ref ItemSlotEjectAttemptEvent ev)
    {
        // oooughhh
        /*if (component.SlotToEjectFrom == ev.Slot)
        {
            Dirty(entity, component);
            return;
        }*/
        // how the fuck am i gonna do this ?__?

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

        VendFromSlot(uid, args.ItemsToEject);
        Dirty(uid, component);
    }

    private void VendFromSlot(EntityUid uid, List<string> itemSlotsToEject, SmartFridgeComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (!this.IsPowered(uid, EntityManager))
        {
            return;
        }

        var slotsToEject = new List<ItemSlot>();

        foreach (var id in itemSlotsToEject)
        {
            var item = _itemSlotsSystem.GetItemOrNull(uid, id);

            if (item == null)
                return;

            if (!_itemSlotsSystem.TryGetSlot(uid, id, out var itemSlot) && itemSlot == null)
                return;

            slotsToEject.Add(itemSlot);
        }

        component.Ejecting = true;
        component.SlotsToEjectFrom = slotsToEject;

        _audio.PlayPvs(component.SoundVend, uid);
    }

    private void EjectItem(EntityUid uid, SmartFridgeComponent component)
    {
        if (component.SlotsToEjectFrom == null)
            return;

        // please work
        foreach (var slot in component.SlotsToEjectFrom)
        {
            _itemSlotsSystem.TryEject(uid, slot, null, out _);
        }
        // it doesnt work
        // it gets to this point fine, it just dispenses 1 instead of many

        component.Inventory = GetInventory(uid);
        component.SlotsToEjectFrom = null;

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
