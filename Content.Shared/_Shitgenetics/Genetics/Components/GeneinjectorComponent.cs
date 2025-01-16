using Robust.Shared.GameStates;

namespace Content.Shared.Genetics.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedGeneSystem))]
public sealed partial class GeneinjectorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float InjectTime = 2.5f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StunBonus = 2f;

    [DataField("color"), AutoNetworkedField]
    [Access(Other = AccessPermissions.ReadWrite)]
    public Color Color = Color.Coral;
}

