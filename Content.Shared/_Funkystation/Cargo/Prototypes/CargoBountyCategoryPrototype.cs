using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Cargo.Prototypes;

/// <summary>
/// Bounty Category used to create the list of possible items a bounty can pull from for the defined category
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed partial class CargoBountyCategoryPrototype : IPrototype
{
    /// <summary>
    /// ID of the category
    /// </summary>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Player facing name of the category
    /// </summary>
    [DataField(required: true)]
    public LocId Name { get; private set; }

    /// <summary>
    /// List of possible entities the bounty can be created using
    /// </summary>
    [DataField(required: true)]
    public required List<CargoBountyItemEntry> Entries;

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "NT";
}
