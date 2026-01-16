namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationStupefactionComponent : Component
{
    /// <summary>
    /// Minimum time between stuns (in seconds)
    /// </summary>
    [DataField]
    public float MinInterval = 120f;

    /// <summary>
    /// Maximum time between stuns (in seconds)
    /// </summary>
    [DataField]
    public float MaxInterval = 180f;

    /// <summary>
    /// How much stamina damage to deal (overkill just guarantees full drain)
    /// </summary>
    [DataField]
    public float DrainAmount = 999f;

    [ViewVariables]
    public TimeSpan NextDrainTime;
}
