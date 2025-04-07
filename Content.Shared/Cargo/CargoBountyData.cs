using Robust.Shared.Serialization;
using Content.Shared.Cargo.Prototypes;
using Content.Shared.Whitelist;

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
            items.Add(new CargoBountyItemData(entry));
        }
        Entries = items;
    }

    public CargoBountyData()
    {

    }
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class CargoBountyItemData
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

    public CargoBountyItemData(CargoBountyItemEntry entry)
    {
        Name = entry.Name;
        Amount = entry.Amount;
        Whitelist = entry.Whitelist;
        Blacklist = entry.Blacklist;
    }
}
