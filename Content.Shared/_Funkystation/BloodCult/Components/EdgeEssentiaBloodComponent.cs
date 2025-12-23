using Content.Shared.Chemistry.Components;

namespace Content.Shared.BloodCult;

[RegisterComponent]
public sealed partial class EdgeEssentiaBloodComponent : Component
{
    [DataField]
    public Solution? AppliedBloodOverride;

    [DataField]
    public bool Active;

    [DataField]
    public Solution OriginalBloodReagents = new();
}
