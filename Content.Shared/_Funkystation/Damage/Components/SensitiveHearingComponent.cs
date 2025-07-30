namespace Content.Shared.Damage.Components;

/// <summary>
/// Tracks how much damage an entity got from loud noises around it.
/// </summary>
[RegisterComponent]
public sealed partial class SensitiveHearingComponent : Component
{
    /// <summary>
    /// DamageAmount
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("DamageAmount")]
    public float damageAmount = 0.0f;

    [ViewVariables(VVAccess.ReadWrite), DataField("DeafnessThreshold")]
    public float deafnessThreshold = 100.0f;

    public bool IsDeaf { get => (damageAmount >= deafnessThreshold); }
}

