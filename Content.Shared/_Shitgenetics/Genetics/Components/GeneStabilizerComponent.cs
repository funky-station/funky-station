using Robust.Shared.GameStates;

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGeneStabilizerSystem))]
public sealed partial class GeneStabilizerComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 1.25f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 1f;
}

