// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Gansu <68031780+GansuLalan@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 aa5g21 <aa5g21@soton.ac.uk>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;
using Content.Shared.Cargo.Prototypes;

namespace Content.Shared.Cargo;

/// <summary>
/// A data structure for storing currently available bounties.
/// </summary>
[DataDefinition, NetSerializable, Serializable]
public sealed partial class CargoBountyData
{
    /// <summary>
    /// A unique id used to identify the bounty
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The monetary reward for completing the bounty
    /// </summary>
    [DataField(required: true)]
    public int Reward;

    /// <summary>
    /// A description for flavour purposes.
    /// </summary>
    [DataField]
    public LocId Description = string.Empty;

    /// <summary>
    /// The entries that must be satisfied for the cargo bounty to be complete.
    /// </summary>
    [DataField(required: true)]
    public List<CargoBountyItemData> Entries = new();

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "NT";

    public LocId Category;

    public CargoBountyData(int uniqueIdentifier, int reward, LocId description, List<CargoBountyItemData> entries, string idPrefix = "NT")
    {
        Id = $"{IdPrefix}{uniqueIdentifier:D3}";
        Reward = reward;
        Description = description;
        Entries = entries;
        IdPrefix = idPrefix;
    }

    /// <summary>
    /// Used for creating bounties via the old system with pre-defined bounties
    /// </summary>
    /// <param name="uniqueIdentifier">Some number to be used as an ID with IdPrefix</param>
    /// <param name="prototype">The prototype of the bounty to be created</param>
    public CargoBountyData(int uniqueIdentifier, CargoBountyPrototype prototype)
    {
        Id = $"{IdPrefix}{uniqueIdentifier:D3}";
        Description = prototype.Description;
        IdPrefix = prototype.IdPrefix;
        Reward = prototype.Reward;
        var items = new List<CargoBountyItemData>();
        foreach (var entry in prototype.Entries)
        {
            CargoBountyItemData newItem = entry switch
            {
                CargoObjectBountyItemEntry itemEntry => new CargoObjectBountyItemData(itemEntry),
                CargoReagentBountyItemEntry itemEntry => new CargoReagentBountyItemData(itemEntry),
                _ => throw new NotImplementedException($"Unknown type: {entry.GetType().Name}"),
            };
            items.Add(newItem);
        }
        Entries = items;
    }

    public CargoBountyData()
    {

    }
}
