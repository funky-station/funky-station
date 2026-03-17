using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float TimeWithoutSmoking = 0f;

}



