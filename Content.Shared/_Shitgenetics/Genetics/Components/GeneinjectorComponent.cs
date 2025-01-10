using Robust.Shared.GameStates;

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent]
[Access(typeof(SharedGeneSystem))]
public sealed partial class GeneinjectorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 2.5f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 2f;

    /// <summary>
    /// An opptional color specification for <see cref="BodyIconState"/>
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Color Color = Color.White;
}

