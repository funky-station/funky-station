namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Mutation component that grants passive brute and burn regeneration.
/// </summary>
[RegisterComponent]
public sealed partial class MutationRegenerationComponent : Component
{
    /// <summary>
    /// Amount of brute/burn healed per interval.
    /// </summary>
    [DataField]
    public float HealAmount = 1.0f;

    /// <summary>
    /// Healing interval in seconds.
    /// </summary>
    [DataField]
    public float Interval = 1.0f;

    [ViewVariables]
    public TimeSpan NextHeal;
}
