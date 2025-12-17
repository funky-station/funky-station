namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Gives the user a buff to stamina
/// </summary>
[RegisterComponent]
public sealed partial class MutationIncreaseStaminaCritThresholdComponent : Component
{
    [DataField]
    public float ThresholdBonus = 30f;  // +30 = 130 threshold (30% harder to stam-crit)
}
