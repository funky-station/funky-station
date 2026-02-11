using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationInstabilityPolymorphComponent : Component
{
/// <summary>
    /// The ID of the polymorph prototype to apply (e.g. "GeneticMonkeyMorph", "GeneticFleshJaredMorph", etc.)
    /// </summary>
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> PolymorphId = "GeneticMonkeyMorph";
}
