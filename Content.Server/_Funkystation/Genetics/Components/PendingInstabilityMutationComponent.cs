namespace Content.Server._Funkystation.Genetics.Components;

[RegisterComponent]
public sealed partial class PendingInstabilityMutationComponent : Component
{
    public string MutationId = string.Empty;
    public TimeSpan EndTime;
    public TimeSpan StartTime;
    public bool WarningStart = false;
    public bool WarningHalfway = false;
    public bool Warning10Sec = false;
}
