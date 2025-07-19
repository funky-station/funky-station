// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cargo.Prototypes;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo;

[ImplicitDataDefinitionForInheritors, Serializable, NetSerializable]
public abstract partial class CargoBountyItemData
{
    /// <summary>
    /// How much of the item must be present to satisfy the entry
    /// </summary>
    [DataField]
    public int Amount { get; set; } = 1;

    /// <summary>
    /// A player-facing name for the item.
    /// </summary>
    [DataField]
    public LocId Name { get; set; } = string.Empty;
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class CargoObjectBountyItemData : CargoBountyItemData
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
    public EntityWhitelist? Blacklist { get; set; }

    // todo: implement some kind of simple generic condition system

    public CargoObjectBountyItemData(CargoObjectBountyItemEntry entry)
    {
        Name = entry.Name;
        Amount = entry.Amount;
        Whitelist = entry.Whitelist;
        Blacklist = entry.Blacklist;
    }
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class CargoReagentBountyItemData : CargoBountyItemData
{
    /// <summary>
    /// What reagent will satisfy the bounty requirement
    /// </summary>
    public ProtoId<ReagentPrototype> Reagent;

    public CargoReagentBountyItemData(CargoReagentBountyItemEntry entry)
    {
        Name = entry.Name;
        Amount = entry.Amount;
        Reagent = entry.Reagent;
    }
}
