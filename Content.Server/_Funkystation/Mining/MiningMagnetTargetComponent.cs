namespace Content.Server._Funkystation.Mining;

/// <summary>
/// Indicates the entity is a salvage target for tracking.
/// </summary>
[RegisterComponent]
public sealed partial class MiningMagnetTargetComponent : Component
{
    /// <summary>
    /// Entity that spawned us.
    /// </summary>
    [DataField]
    public EntityUid DataTarget;
}
