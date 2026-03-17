using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    // How frequently (in seconds) should the user smoke
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SmokingInterval = 60f;

    //the current stage of withdrawal
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int WithdrawalStage;



}



