using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    // How frequently (in FrameTime) should the user smoke
    [DataField]
    public float SmokingInterval =145;

    [DataField]
    public float CurrentSmokingInterval =0f;

    //the current stage of withdrawal
    [DataField]
    public int WithdrawalStage;

    //how much nicotine is in the system.
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public FixedPoint2 CurrentNicotineLevel;

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float NewNicotineLevel;

    //how long since entity last smoked (in FrameTime)
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float TimeSinceSmoking;

    /*[DataField]
    public List<string> LastStageMessages =
    [
        "YOUR HEAD IS KILLING YOU!", "YOUR WHOLE BODY CRAVES NICOTINE!","You feel VERY restless!",
        "It feels like the whole WORLD'S falling down!",
    ];
    */





}



