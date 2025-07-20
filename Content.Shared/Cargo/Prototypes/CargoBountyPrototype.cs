// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 aa5g21 <aa5g21@soton.ac.uk>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chemistry.Reagent;
using Content.Shared.Research.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Prototypes;

/// <summary>
/// This is a prototype for a cargo bounty, a set of items
/// that must be sold together in a labeled container in order
/// to receive a monetary reward.
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed partial class CargoBountyPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The monetary reward for completing the bounty
    /// </summary>
    [DataField(required: true)]
    public int Reward;

    /// <summary>
    /// A description for flava purposes.
    /// </summary>
    [DataField]
    public LocId Description = string.Empty;

    /// <summary>
    /// The entries that must be satisfied for the cargo bounty to be complete.
    /// </summary>
    [DataField(required: true)]
    public List<CargoBountyItemEntry> Entries = new();

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "NT";
}

[ImplicitDataDefinitionForInheritors, Serializable]
public abstract partial record CargoBountyItemEntry
{
    /// <summary>
    /// How much of the item must be present to satisfy the entry
    /// </summary>
    [DataField]
    public int Amount { get; set; } = 1;

    // Beginning of Funky Station edits
    /// <summary>
    /// A minimum amount of the item that can be requested in a bounty, used to make sure a bounty isn't to underwhelming
    /// </summary>
    [DataField]
    public int MinAmount { get; set; } = 1;

    /// <summary>
    /// A maximum amount of the item that can be requested for a bounty
    /// </summary>
    [DataField]
    public int MaxAmount { get; set; } = 1;

    /// <summary>
    /// The step size for the bounties amount, i.e. min:1 max:3 step:2 means only amounts 1 and 3 will be generated.
    /// </summary>
    [DataField]
    public int AmountStep { get; set; } = 1;

    /// <summary>
    /// The amount each item will reward for a bounty
    /// </summary>
    [DataField]
    public int RewardPer { get; set; } = 1;

    /// <summary>
    /// A player-facing name for the item. Assigned here but declared in the cargo bounties.ftl file.
    /// </summary>
    [DataField]
    public LocId Name { get; set; } = string.Empty;

    /// <summary>
    /// Some weight that can be used to effect the chances an item is selected, default is 1, smaller number means less
    /// likely, higher more likely.
    /// </summary>
    [DataField]
    public double Weight { get; set; } = 1;
}

[DataDefinition, Serializable, NetSerializable]
public partial record CargoObjectBountyItemEntry : CargoBountyItemEntry
{
    /// <summary>
    /// A whitelist for determining what items satisfy the entry.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist { get; set; } = default!;

    /// <summary>
    /// A blacklist that can be used to exclude items in the whitelist.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist { get; set; } = null;

    // todo: implement some kind of simple generic condition system

    [DataField]
    public List<ProtoId<TechnologyPrototype>>? RequiredResearch { get; set; }
}

[DataDefinition, Serializable, NetSerializable]
public partial record CargoReagentBountyItemEntry : CargoBountyItemEntry
{
    /// <summary>
    /// What reagent will satisfy the entry.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<ReagentPrototype> Reagent { get; set; }
    // End of Funky Station edits
}
