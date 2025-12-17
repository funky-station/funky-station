using Robust.Shared.GameStates;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationSpeedBoostComponent : Component
{
    /// <summary>
    /// Multiplier applied to base walk speed.
    /// </summary>
    [DataField(required: true)]
    public float WalkMultiplier = 1.0f;

    /// <summary>
    /// Multiplier appiled to base sprint speed.
    /// </summary>
    [DataField(required: true)]
    public float SprintMultiplier = 1.0f;
}
