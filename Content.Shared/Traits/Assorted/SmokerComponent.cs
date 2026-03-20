using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    // How frequently (in FrameTime) should the user smoke
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float SmokingInterval =60f;

    //the current stage of withdrawal
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int WithdrawalStage;

    //how much nicotine is in the system.
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public FixedPoint2 CurrentNicotineLevel;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float NewNicotineLevel;

    //how long since entity last smoked (in FrameTime)
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float TimeSinceSmoking;





}



