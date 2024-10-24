using Robust.Shared.GameStates;

namespace Content.Shared.Revolutionary;

[RegisterComponent, NetworkedComponent]
public sealed partial class RevolutionaryFlashOnTriggerComponent : Component
{
    [DataField] public float Range = 1.0f;
    [DataField] public float Duration = 8.0f;
    [DataField] public float Probability = 1.0f;
}
