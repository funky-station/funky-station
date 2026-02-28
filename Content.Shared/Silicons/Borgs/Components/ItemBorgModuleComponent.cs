// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Hands.Components;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Silicons.Borgs.Components;

/// <summary>
/// This is used for a <see cref="BorgModuleComponent"/> that provides items to the entity it's installed into.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedBorgSystem))]
public sealed partial class ItemBorgModuleComponent : Component
{
    /// <summary>
    /// The hands that are provided.
    /// </summary>
    [DataField(required: true)]
    public List<BorgHand> Hands = new();

    /// <summary>
    /// Maps hand IDs (created when the module is selected) to the item entity held in that hand.
    /// Null means items have never been provided yet (first time). EntityUid.Invalid means the hand
    /// has no item (either the <see cref="BorgHand.Item"/> was null, or the item was not found).
    /// </summary>
    [DataField]
    public Dictionary<string, EntityUid>? StoredItems;

    /// <summary>
    /// A counter used to generate unique hand IDs.
    /// </summary>
    [DataField]
    public int HandCounter;

    /// <summary>
    /// An ID for the container where provided items are stored when not in use.
    /// </summary>
    [DataField]
    public string HoldingContainer = "holding_container";
}

/// <summary>
/// Describes a hand slot provided by an <see cref="ItemBorgModuleComponent"/>.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public partial struct BorgHand
{
    /// <summary>
    /// Entity prototype to spawn into the hand when the module is first selected.
    /// Null means the hand is initially empty.
    /// </summary>
    [DataField]
    public EntProtoId? Item;

    /// <summary>
    /// Location of the hand (Left, Middle, Right).
    /// </summary>
    [DataField]
    public HandLocation Location = HandLocation.Middle;

    /// <summary>
    /// If true, the held item can be removed by the borg or others.
    /// </summary>
    [DataField]
    public bool ForceRemovable = false;

    /// <summary>
    /// Items matching this whitelist may be picked up into the hand.
    /// Null means no whitelist restriction.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// Items matching this blacklist may NOT be picked up into the hand.
    /// Null means no blacklist restriction.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;

    /// <summary>
    /// Prototype to display in the UI when the hand is empty.
    /// </summary>
    [DataField]
    public EntProtoId? EmptyRepresentative;

    /// <summary>
    /// Localization key for the label shown when the hand is empty.
    /// </summary>
    [DataField]
    public LocId? EmptyLabel;

    public BorgHand() { }
}
