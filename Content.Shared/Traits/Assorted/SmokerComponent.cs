using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    // How frequently (in seconds) should the user smoke
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SmokingInterval =60f;

    //the current stage of withdrawal
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int WithdrawalStage;

    //how much nicotine is in the system.
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float CurrentNicotineLevel;

    //how long since entity last smoked (in seconds)
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float TimeSinceSmoking;



}



