using Content.Shared.FixedPoint;

[RegisterComponent]
public sealed partial class LeakyObjectComponent : Component
{
    [DataField]
    public string SolutionName = "leaky";
    [DataField]
    public FixedPoint2 TransferAmount = FixedPoint2.New(0.5);
    [DataField]
    public float LeakEfficiency = 0.5f;
    [DataField]
    /// <summary>
    /// how often the object should transfer sulution
    /// </summary>
    public float UpdateTime = 4f;
    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;
}
