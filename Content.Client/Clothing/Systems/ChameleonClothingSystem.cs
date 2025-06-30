using System.Linq;
using Content.Client.PDA;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.Clothing.Systems;

// All valid items for chameleon are calculated on client startup and stored in dictionary.
public sealed class ChameleonClothingSystem : SharedChameleonClothingSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;

    private static readonly SlotFlags[] IgnoredSlots =
    {
        SlotFlags.All,
        SlotFlags.PREVENTEQUIP
    };
    private static readonly SlotFlags[] Slots = Enum.GetValues<SlotFlags>().Except(IgnoredSlots).ToArray();

    private readonly Dictionary<SlotFlags, List<string>> _data = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonClothingComponent, AfterAutoHandleStateEvent>(HandleState);

        PrepareAllVariants();
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnProtoReloaded);
    }

    private void OnProtoReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<EntityPrototype>())
            PrepareAllVariants();
    }

    private void HandleState(EntityUid uid, ChameleonClothingComponent component, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(uid, component);
    }

    protected override void UpdateSprite(EntityUid uid, EntityPrototype proto)
    {
        base.UpdateSprite(uid, proto);
        if (TryComp(uid, out SpriteComponent? sprite)
            && proto.TryGetComponent(out SpriteComponent? otherSprite, _factory))
        {
            sprite.CopyFrom(otherSprite);
        }

        // Edgecase for PDAs to include visuals when UI is open
        if (TryComp(uid, out PdaBorderColorComponent? borderColor)
            && proto.TryGetComponent(out PdaBorderColorComponent? otherBorderColor, _factory))
        {
            borderColor.BorderColor = otherBorderColor.BorderColor;
            borderColor.AccentHColor = otherBorderColor.AccentHColor;
            borderColor.AccentVColor = otherBorderColor.AccentVColor;
        }
    }

    /// <summary>
    ///     Get a list of valid chameleon targets for these slots.
    /// </summary>
    public IEnumerable<string> GetValidTargets(SlotFlags slot)
    {
        var set = new HashSet<string>();
        
        // Handle SlotFlags.NONE specially
        if (slot == SlotFlags.NONE)
        {
            if (_data.ContainsKey(SlotFlags.NONE))
            {
                set.UnionWith(_data[SlotFlags.NONE]);
            }
            return set;
        }
        
        // Handle clothing slots normally
        foreach (var availableSlot in _data.Keys)
        {
            if (availableSlot == SlotFlags.NONE)
                continue; // Skip handheld items for clothing slots
                
            if (slot.HasFlag(availableSlot))
            {
                set.UnionWith(_data[availableSlot]);
            }
        }
        return set;
    }

    private void PrepareAllVariants()
    {
        _data.Clear();
        var prototypes = _proto.EnumeratePrototypes<EntityPrototype>();

        foreach (var proto in prototypes)
        {
            // check if this is valid clothing or handheld item
            if (!IsValidTarget(proto))
                continue;

            // Handle handheld items (SlotFlags.NONE)
            if (proto.TryGetComponent(out ClothingComponent? clothing, _factory))
            {
                // This is a clothing item - handle normally
                foreach (var slot in Slots)
                {
                    if (!clothing.Slots.HasFlag(slot))
                        continue;

                    if (!_data.ContainsKey(slot))
                    {
                        _data.Add(slot, new List<string>());
                    }
                    _data[slot].Add(proto.ID);
                }
            }
            else
            {
                // This is a handheld item - add to NONE slot
                if (!_data.ContainsKey(SlotFlags.NONE))
                {
                    _data.Add(SlotFlags.NONE, new List<string>());
                }
                _data[SlotFlags.NONE].Add(proto.ID);
            }
        }
    }
}
