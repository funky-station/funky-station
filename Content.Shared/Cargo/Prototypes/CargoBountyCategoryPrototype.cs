using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Cargo.Prototypes;

[Prototype, Serializable, NetSerializable]
public sealed partial class CargoBountyCategoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public string Name { get; private set; } = default!;

    [DataField(required: true)]
    public required List<CargoBountyItemEntry> Entries;
}
