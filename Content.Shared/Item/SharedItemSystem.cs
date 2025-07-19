// SPDX-FileCopyrightText: 2021 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Moony <moonheart08@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 Darkie <darksaiyanis@gmail.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Varen <ychwack@hotmail.it>
// SPDX-FileCopyrightText: 2023 Ygg01 <y.laughing.man.y@gmail.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Dia <diatomic.ge@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Verbs;
using Content.Shared.Examine;
using Content.Shared.Inventory;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using JetBrains.Annotations;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Item;

public abstract class SharedItemSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!; // Goobstation
    [Dependency] private readonly SharedStorageSystem _storage = default!; // Goobstation
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private   readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] protected readonly SharedContainerSystem Container = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemComponent, GetVerbsEvent<InteractionVerb>>(AddPickupVerb);
        SubscribeLocalEvent<ItemComponent, InteractHandEvent>(OnHandInteract);
        SubscribeLocalEvent<ItemComponent, AfterAutoHandleStateEvent>(OnItemAutoState);

        SubscribeLocalEvent<ItemComponent, ExaminedEvent>(OnExamine);

        SubscribeLocalEvent<ItemToggleSizeComponent, ItemToggledEvent>(OnItemToggle);
    }

    private void OnItemAutoState(EntityUid uid, ItemComponent component, ref AfterAutoHandleStateEvent args)
    {
        SetHeldPrefix(uid, component.HeldPrefix, force: true, component);
    }

    #region Public API

    public void SetSize(EntityUid uid, ProtoId<ItemSizePrototype> size, ItemComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        component.Size = size;
        Dirty(uid, component);
    }

    public void SetShape(EntityUid uid, List<Box2i>? shape, ItemComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        component.Shape = shape;
        Dirty(uid, component);
    }

    public void SetHeldPrefix(EntityUid uid, string? heldPrefix, bool force = false, ItemComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (!force && component.HeldPrefix == heldPrefix)
            return;

        component.HeldPrefix = heldPrefix;
        Dirty(uid, component);
        VisualsChanged(uid);
    }

    /// <summary>
    ///     Copy all item specific visuals from another item.
    /// </summary>
    public void CopyVisuals(EntityUid uid, ItemComponent otherItem, ItemComponent? item = null)
    {
        if (!Resolve(uid, ref item))
            return;

        item.RsiPath = otherItem.RsiPath;
        item.InhandVisuals = otherItem.InhandVisuals;
        item.HeldPrefix = otherItem.HeldPrefix;

        Dirty(uid, item);
        VisualsChanged(uid);
    }

    #endregion

    private void OnHandInteract(EntityUid uid, ItemComponent component, InteractHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = _handsSystem.TryPickup(args.User, uid, animateUser: false);
    }

    private void AddPickupVerb(EntityUid uid, ItemComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (args.Hands == null ||
            args.Using != null ||
            !args.CanAccess ||
            !args.CanInteract ||
            !_handsSystem.CanPickupAnyHand(args.User, args.Target, handsComp: args.Hands, item: component))
            return;

        InteractionVerb verb = new();
        verb.Act = () => _handsSystem.TryPickupAnyHand(args.User, args.Target, checkActionBlocker: false,
            handsComp: args.Hands, item: component);
        verb.Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/pickup.svg.192dpi.png"));

        // if the item already in a container (that is not the same as the user's), then change the text.
        // this occurs when the item is in their inventory or in an open backpack
        Container.TryGetContainingContainer((args.User, null, null), out var userContainer);
        if (Container.TryGetContainingContainer((args.Target, null, null), out var container) && container != userContainer)
            verb.Text = Loc.GetString("pick-up-verb-get-data-text-inventory");
        else
            verb.Text = Loc.GetString("pick-up-verb-get-data-text");

        args.Verbs.Add(verb);
    }

    private void OnExamine(EntityUid uid, ItemComponent component, ExaminedEvent args)
    {
        // show at end of message generally
        args.PushMarkup(Loc.GetString("item-component-on-examine-size",
            ("size", GetItemSizeLocale(component.Size))), priority: -1);
    }

    public ItemSizePrototype GetSizePrototype(ProtoId<ItemSizePrototype> id)
    {
        return _prototype.Index(id);
    }

    /// <summary>
    ///     Notifies any entity that is holding or wearing this item that they may need to update their sprite.
    /// </summary>
    /// <remarks>
    ///     This is used for updating both inhand sprites and clothing sprites, but it's here just cause it needs to
    ///     be in one place.
    /// </remarks>
    public virtual void VisualsChanged(EntityUid owner)
    {
    }

    [PublicAPI]
    public string GetItemSizeLocale(ProtoId<ItemSizePrototype> size)
    {
        return Loc.GetString(GetSizePrototype(size).Name);
    }

    [PublicAPI]
    public int GetItemSizeWeight(ProtoId<ItemSizePrototype> size)
    {
        return GetSizePrototype(size).Weight;
    }

    /// <summary>
    /// Gets the default shape of an item.
    /// </summary>
    public IReadOnlyList<Box2i> GetItemShape(Entity<ItemComponent?> uid)
    {
        if (!Resolve(uid, ref uid.Comp))
            return new Box2i[] { };

        return uid.Comp.Shape ?? GetSizePrototype(uid.Comp.Size).DefaultShape;
    }

    /// <summary>
    /// Gets the default shape of an item.
    /// </summary>
    public IReadOnlyList<Box2i> GetItemShape(ItemComponent component)
    {
        return component.Shape ?? GetSizePrototype(component.Size).DefaultShape;
    }

    /// <summary>
    /// Gets the shape of an item, adjusting for rotation and offset.
    /// </summary>
    public IReadOnlyList<Box2i> GetAdjustedItemShape(Entity<ItemComponent?> entity, ItemStorageLocation location)
    {
        return GetAdjustedItemShape(entity, location.Rotation, location.Position);
    }

    /// <summary>
    /// Gets the shape of an item, adjusting for rotation and offset.
    /// </summary>
    public IReadOnlyList<Box2i> GetAdjustedItemShape(Entity<ItemComponent?> entity, Angle rotation, Vector2i position)
    {
        if (!Resolve(entity, ref entity.Comp))
            return new Box2i[] { };

        var shapes = GetItemShape(entity);
        var boundingShape = shapes.GetBoundingBox();
        var boundingCenter = ((Box2) boundingShape).Center;
        var matty = Matrix3Helpers.CreateTransform(boundingCenter, rotation);
        var drift = boundingShape.BottomLeft - matty.TransformBox(boundingShape).BottomLeft;

        var adjustedShapes = new List<Box2i>();
        foreach (var shape in shapes)
        {
            var transformed = matty.TransformBox(shape).Translated(drift);
            var floored = new Box2i(transformed.BottomLeft.Floored(), transformed.TopRight.Floored());
            var translated = floored.Translated(position);

            adjustedShapes.Add(translated);
        }

        return adjustedShapes;
    }

    /// <summary>
    /// Used to update the Item component on item toggle (specifically size).
    /// </summary>
    private void OnItemToggle(EntityUid uid, ItemToggleSizeComponent itemToggleSize, ItemToggledEvent args)
    {
        if (!TryComp(uid, out ItemComponent? item))
            return;

        if (args.Activated)
        {
            if (itemToggleSize.ActivatedShape != null)
            {
                // Set the deactivated shape to the default item's shape before it gets changed.
                itemToggleSize.DeactivatedShape ??= new List<Box2i>(GetItemShape(item));
                SetShape(uid, itemToggleSize.ActivatedShape, item);
            }

            if (itemToggleSize.ActivatedSize != null)
            {
                // Set the deactivated size to the default item's size before it gets changed.
                itemToggleSize.DeactivatedSize ??= item.Size;
                SetSize(uid, (ProtoId<ItemSizePrototype>) itemToggleSize.ActivatedSize, item);
            }
        }
        else
        {
            if (itemToggleSize.DeactivatedShape != null)
            {
                SetShape(uid, itemToggleSize.DeactivatedShape, item);
            }

            if (itemToggleSize.DeactivatedSize != null)
            {
                SetSize(uid, (ProtoId<ItemSizePrototype>) itemToggleSize.DeactivatedSize, item);
            }
        }

        if (Container.TryGetContainingContainer((uid, null, null), out var container) &&
            !_handsSystem.IsHolding(container.Owner, uid)) // Funkystation - Don't move items in hands.
        {
            // Funkystation - Check if the item is in a pocket.
            var wasInPocket = false;
            if (_inventory.TryGetContainerSlotEnumerator(container.Owner, out var enumerator, SlotFlags.POCKET))
            {
                while (enumerator.NextItem(out var slotItem, out var slot))
                {
                    if (slotItem == uid)
                    {
                        // Funkystation - We found it in a pocket.
                        wasInPocket = true;

                        if (!_inventory.CanEquip(container.Owner, uid, slot.Name, out var _, slot))
                        {
                            // Funkystation - It no longer fits, so try to hand it to whoever toggled it.
                            _transform.AttachToGridOrMap(uid);
                            _handsSystem.PickupOrDrop(args.User, uid, animate: true);
                        }
                        break;
                    }
                }
            }

            if (!wasInPocket && TryComp(container.Owner,
                out StorageComponent? storage)) // Goobstation - reinsert item in storage because size changed
            {
                _transform.AttachToGridOrMap(uid);
                if (!_storage.Insert(container.Owner, uid, out _, null, storage, false))
                {
                    // Funkystation - It didn't fit, so try to hand it to whoever toggled it.
                    _handsSystem.PickupOrDrop(args.User, uid, animate: false);
                }
            }
        }

        Dirty(uid, item);
    }
}
