using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Revolutionary.Prototypes;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class HeadRevolutionaryRecipePrototype : IPrototype
{
    [DataField]
    public List<string>? Recipes;

    [IdDataField]
    public string ID { get; private set; } = default!;
}
