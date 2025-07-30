namespace Content.Shared.Damage.Components;

/// <summary>
/// Tracks how much damage an entity got from loud noises around it.
/// </summary>
[RegisterComponent]
public sealed partial class SensitiveHearingComponent : Component
{
    /// <summary>
    /// Controls whether the eardrum rupture message have been shown or not.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField("RuptureFlag")]
    public bool RuptureFlag;

    [ViewVariables(VVAccess.ReadWrite), DataField("DamageAmount")]
    public float DamageAmount
    {
        get => _damage;
        set {
            _damage = value;
            if (_damage < DeafnessThreshold)
                RuptureFlag = false;
        }
    }

    private float _damage;

    /// <summary>
    /// When damage reaches this value - entity goes deaf.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("WarningThreshold")]
    public float WarningThreshold = 50.0f;

    /// <summary>
    /// When damage reaches this value - entity goes deaf.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("DeafnessThreshold")]
    public float DeafnessThreshold = 100.0f;

    public bool IsDeaf {get => (DamageAmount >= DeafnessThreshold); }
}

