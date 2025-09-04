using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;

namespace Content.Shared.MalfAI;

/// <summary>
/// Component for Malf AI sabotage objectives (doomsday, assassinate, protect).
/// Uses standard TargetObjectiveComponent for target tracking.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MalfAiSabotageObjectiveComponent : Component
{
    /// <summary>
    /// Type of sabotage objective: "doomsday", "assassinate", or "protect"
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string SabotageType = string.Empty;
}
