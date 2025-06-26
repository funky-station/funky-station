using Content.Server.IdentityManagement;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.IdentityManagement.Components;
using Robust.Shared.Prototypes;
using Content.Server.Speech.Components;
using Content.Shared.Armor;
using Content.Shared._Shitmed.Body.Part;
using Content.Shared.Clothing;
using Robust.Shared.GameObjects;
using Robust.Shared.Containers;
using Content.Shared.Actions; 
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Utility;

namespace Content.Server.Clothing.Systems;

public sealed class ChameleonClothingSystem : SharedChameleonClothingSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IComponentFactory _factory = default!;
    [Dependency] private readonly IdentitySystem _identity = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChameleonClothingComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ChameleonClothingComponent, ChameleonPrototypeSelectedMessage>(OnSelected);
    }

    private void OnMapInit(EntityUid uid, ChameleonClothingComponent component, MapInitEvent args)
    {
        SetSelectedPrototype(uid, component.Default, forceUpdate: true, component);
    }

    private void OnSelected(EntityUid uid, ChameleonClothingComponent component, ChameleonPrototypeSelectedMessage args)
    {
        SetSelectedPrototype(uid, args.SelectedId, component: component);
    }

    private void UpdateUi(EntityUid uid, ChameleonClothingComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var state = new ChameleonBoundUserInterfaceState(component.Slot, component.Default, component.RequireTag);
        UI.SetUiState(uid, ChameleonUiKey.Key, state);
    }

    /// <summary>
    /// Changes the chameleon item's name, description, and sprite to mimic another entity prototype.
    /// </summary>
    public void SetSelectedPrototype(EntityUid uid, string? protoId, bool forceUpdate = false,
        ChameleonClothingComponent? component = null)
    {
        if (!Resolve(uid, ref component, false))
            return;

        if (component.Default == protoId && !forceUpdate)
            return;

        if (string.IsNullOrEmpty(protoId) || !_proto.TryIndex(protoId, out EntityPrototype? proto))
            return;

        if (!IsValidTarget(proto, component.Slot, component.RequireTag))
            return;

        // Reset armor perception properties
        component.PerceivedArmorModifiers = null;
        component.PerceivedArmourModifiersHidden = true;
        component.PerceivedShowArmorOnExamine = false;

        // Remove existing accent component and handle unequip if necessary
        if (TryComp<AddAccentClothingComponent>(uid, out var oldAccent) && 
            oldAccent.IsActive && 
            component.User != null)
        {
            if (TryComp<ClothingComponent>(uid, out var clothing))
            {
                var unequipEvent = new ClothingGotUnequippedEvent(component.User.Value, clothing);
                RaiseLocalEvent(uid, ref unequipEvent);
            }
        }
        RemComp<AddAccentClothingComponent>(uid);

        // Copy armor properties from the target prototype
        if (proto.TryGetComponent<ArmorComponent>(out var armorComp))
        {
            component.PerceivedArmorModifiers = armorComp.Modifiers;
            component.PerceivedShowArmorOnExamine = armorComp.ShowArmorOnExamine;
        }

        // Copy accent component from the target prototype
        if (proto.TryGetComponent<AddAccentClothingComponent>(out var accentComp))
        {
            var newAccent = AddComp<AddAccentClothingComponent>(uid);
            newAccent.Accent = accentComp.Accent;
            newAccent.ReplacementPrototype = accentComp.ReplacementPrototype;

            // Apply accent immediately if the item is currently equipped
            if (component.User != null && TryComp<ClothingComponent>(uid, out var clothing))
            {
                var equipEvent = new ClothingGotEquippedEvent(component.User.Value, clothing);
                RaiseLocalEvent(uid, ref equipEvent);
            }
        }

        component.Default = protoId;
        UpdateIdentityBlocker(uid, component, proto);
        UpdateVisuals(uid, component);
        UpdateUi(uid, component);
        Dirty(uid, component);
    }

    private void UpdateIdentityBlocker(EntityUid uid, ChameleonClothingComponent component, EntityPrototype proto)
    {
        if (proto.TryGetComponent<IdentityBlockerComponent>(out _, _factory))
            EnsureComp<IdentityBlockerComponent>(uid);
        else
            RemComp<IdentityBlockerComponent>(uid);

        if (component.User != null)
            _identity.QueueIdentityUpdate(component.User.Value);
    }
}
