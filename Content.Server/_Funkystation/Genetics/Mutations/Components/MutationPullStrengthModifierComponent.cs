namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Gives the user a buff to pulling speed
/// </summary>
[RegisterComponent]
public sealed partial class MutationPullStrengthModifierComponent : Component
{
    [DataField("multiplier", required: true)]
    [ViewVariables(VVAccess.ReadWrite)]
    public float PullSlowdownMultiplier = 1.0f;
}
