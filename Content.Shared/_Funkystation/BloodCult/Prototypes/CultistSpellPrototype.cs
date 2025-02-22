using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.BloodCult.Prototypes;

[Serializable, NetSerializable, DataDefinition]
[Prototype("cultAbility")]
public sealed partial class CultAbilityPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    ///     What event should be raised
    /// </summary>
    [DataField] public object? Event;

    /// <summary>
    ///     What actions should be given
    /// </summary>
    [DataField] public List<EntProtoId>? ActionPrototypes;
}
