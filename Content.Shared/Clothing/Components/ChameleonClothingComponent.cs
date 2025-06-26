using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Content.Shared.Body.Part;
using Content.Shared.Armor;
using Content.Shared.Damage;

namespace Content.Shared.Clothing.Components;

/// <summary>
///     Allow players to change clothing sprite to any other clothing prototype.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedChameleonClothingSystem))]
public sealed partial class ChameleonClothingComponent : Component
{
 /// <summary>
 ///     Filter possible chameleon options by their slot flag.
 /// </summary>
 [ViewVariables(VVAccess.ReadOnly)]
 [DataField(required: true)]
public SlotFlags Slot;

 /// <summary>
 ///     EntityPrototype id that chameleon item is trying to mimic.
 /// </summary>
 [ViewVariables(VVAccess.ReadOnly)]
 [DataField(required: true), AutoNetworkedField]
public EntProtoId? Default;

 /// <summary>
 ///     Current user that wears chameleon clothing.
 /// </summary>
 [ViewVariables]
public EntityUid? User;

 /// <summary>
 ///     Filter possible chameleon options by a tag in addition to WhitelistChameleon.
 /// </summary>
 [DataField]
public string? RequireTag;

    /// <summary>
    ///     Perceived armor coverage for examine text.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public List<BodyPartType>? PerceivedArmorCoverage;

    /// <summary>
    ///     Perceived armor modifiers for examine text.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public DamageModifierSet? PerceivedArmorModifiers;

    /// <summary>
    ///     Whether to hide perceived armor coverage in examine.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public bool PerceivedArmourCoverageHidden;

    /// <summary>
    ///     Whether to hide perceived armor modifiers in examine.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public bool PerceivedArmourModifiersHidden;

    /// <summary>
    ///     Whether to show perceived armor in examine.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public bool PerceivedShowArmorOnExamine;

    /// <summary>
    ///     Perceived accent for speech.
    /// </summary>
    [ViewVariables]
    [AutoNetworkedField]
    public string? PerceivedAccent;
}


[Serializable, NetSerializable]
public sealed class ChameleonBoundUserInterfaceState : BoundUserInterfaceState
{
public readonly SlotFlags Slot;
public readonly string? SelectedId;
public readonly string? RequiredTag;

public ChameleonBoundUserInterfaceState(SlotFlags slot, string? selectedId, string? requiredTag)
 {
Slot = slot;
SelectedId = selectedId;
RequiredTag = requiredTag;
 }
}

[Serializable, NetSerializable]
public sealed class ChameleonPrototypeSelectedMessage : BoundUserInterfaceMessage
{
public readonly string SelectedId;

public ChameleonPrototypeSelectedMessage(string selectedId)
 {
SelectedId = selectedId;
 }
}

[Serializable, NetSerializable]
public enum ChameleonUiKey : byte
{
    Key
}
