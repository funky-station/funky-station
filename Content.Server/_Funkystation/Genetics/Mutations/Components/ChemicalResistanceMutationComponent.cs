using Content.Shared.Body.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class ChemicalResistanceMutationComponent : Component
{
    [DataField]
    public ProtoId<MetabolizerTypePrototype> Reagent = "Ethanol";
}
