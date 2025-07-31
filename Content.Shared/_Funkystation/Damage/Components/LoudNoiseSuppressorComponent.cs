using Robust.Shared.Serialization;

namespace Content.Shared.Damage.Components;

/// <summary>
/// Allows certain entities to have loud noise suppression.
/// </summary>
[RegisterComponent]
public sealed partial class LoudNoiseSuppressorComponent : Component
{
    /// <summary>
    /// How good is this entity at suppressing loud noises.
    /// 1.0f - Full suppression.
    /// 0.0f - No suppression.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("SuppressionModifier")]
    public float SuppressionModifier = 1.0f;
}
