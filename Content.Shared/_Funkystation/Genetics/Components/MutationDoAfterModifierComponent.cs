using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Genetics.Mutations.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MutationDoAfterModifierComponent : Component
{
    /// <summary>
    /// Multiplier applied to DoAfter delay times.
    /// </summary>
    [DataField(required: true)]
    public float Multiplier { get; private set; } = 1f;
}
