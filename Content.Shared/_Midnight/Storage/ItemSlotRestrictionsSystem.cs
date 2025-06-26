using System.Linq;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Item;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;

namespace Content.Shared._Midnight.Storage;

public sealed class ItemSlotRestrictionsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSlotRestrictionsComponent, ItemSlotInsertAttemptEvent>(OnInsertAttempt, 
            after: new[] { typeof(ItemSlotsSystem) });
    }

    private void OnInsertAttempt(EntityUid uid, ItemSlotRestrictionsComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!CheckSizeRestrictions(component, args.Item) || !CheckTagRestrictions(component, args.Item))
        {
            args.Cancelled = true;
        }
    }

    private bool CheckSizeRestrictions(ItemSlotRestrictionsComponent component, EntityUid item)
    {
        if (!TryComp<ItemComponent>(item, out var itemComp))
            return true;

        if (!_proto.TryIndex(itemComp.Size, out ItemSizePrototype? itemSize))
            return true;

        // Check max size restriction
        if (component.MaxSize != null &&
            _proto.TryIndex(component.MaxSize.Value, out ItemSizePrototype? maxSize) &&
            itemSize.Weight > maxSize.Weight)
        {
            return false;
        }

        // Check blacklisted sizes
        if (component.BlacklistedSizes?.Contains(itemComp.Size) == true)
        {
            return false;
        }

        return true;
    }

    private bool CheckTagRestrictions(ItemSlotRestrictionsComponent component, EntityUid item)
    {
        if (component.BlacklistedTags == null)
            return true;

        return !component.BlacklistedTags.Any(tag => _tagSystem.HasTag(item, tag));
    }
}

/// <summary>
/// Adds combined restrictions to an item slot
/// </summary>
[RegisterComponent]
public sealed partial class ItemSlotRestrictionsComponent : Component
{
    /// <summary>
    /// Maximum allowed size prototype ID
    /// </summary>
    [DataField("maxSize")]
    public ProtoId<ItemSizePrototype>? MaxSize;

    /// <summary>
    /// Blacklisted size prototype IDs
    /// </summary>
    [DataField("blacklistedSizes")]
    public List<ProtoId<ItemSizePrototype>>? BlacklistedSizes;

    /// <summary>
    /// Blacklisted tag IDs
    /// </summary>
    [DataField("blacklistedTags")]
    public List<string>? BlacklistedTags;
}