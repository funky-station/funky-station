using Robust.Shared.GameStates;

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGeneExtractorSystem))]
public sealed partial class GeneExtractorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 1.25f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 1f;
}

