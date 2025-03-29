using Robust.Shared.GameStates;

namespace Content.Shared.Traits.BrittleBones;

/// <summary>
/// Component that makes an entity enter critical condition sooner due to brittle bones
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BrittleBonesComponent : Component
{
    /// <summary>
    /// How much to modify the critical health threshold by.
    /// Negative values mean entering crit sooner.
    /// </summary>
    [DataField("criticalThresholdModifier"), AutoNetworkedField]
    public float CriticalThresholdModifier = -50f;
}
