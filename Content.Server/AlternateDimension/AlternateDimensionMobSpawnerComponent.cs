using Content.Shared.AlternateDimension;
using Robust.Shared.Prototypes;

namespace Content.Server.AlternateDimension;

/// <summary>
/// Automatically spawns mobs in the specified alternate reality.
/// </summary>
[RegisterComponent]
public sealed partial class AlternateDimensionMobSpawnerComponent : Component
{
    [DataField(required: true)]
    public ProtoId<AlternateDimensionPrototype> TargetDimension;

    /// <summary>
    /// Mob spawns are restricted to being outside of a circle with a diameter proportional to station's AABB
    /// centered on the exit portal.
    /// </summary>
    [DataField("radiusModifier", required: true)]
    public float RadiusModifier;

    [DataField]
    public int Min = 0;

    [DataField]
    public int Max = Int32.MaxValue;

    [DataField]
    public float PlayerScaling = 1.0f;
}
