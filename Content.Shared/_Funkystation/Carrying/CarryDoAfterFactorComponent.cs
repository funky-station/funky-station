using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Carrying;

[RegisterComponent, NetworkedComponent]
public sealed partial class CarryDoAfterFactorComponent: Component
{
    /// <summary>
    /// The number to multiply the carrying doafter speed by (e.g. 0.5 will make it twice as fast.)
    /// </summary>
    [DataField]
    public float Factor = 1;
}
