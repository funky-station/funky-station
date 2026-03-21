using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

[RegisterComponent, NetworkedComponent]
public sealed partial class SmokerComponent : Component
{
    /// <summary>
    /// Time between triggering withdrawal stages when not smoking.
    /// </summary>
    [DataField("WithdrawalInterval",required: true)]
    public float WithdrawalInterval =185f;

    /// <summary>
    /// Timer added to the total timer after triggering a WithdrawalStage increase.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public float NextWithdrawalTime =0f;

    /// <summary>
    /// The current stage of withdrawal of the user. It will determine the effects of the withdrawal and the frequency
    /// of the effects.
    /// </summary>
    [DataField]
    public int WithdrawalStage;

    /// <summary>
    /// Current nicotine levels inside the user.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public FixedPoint2 CurrentNicotineLevel;

    /// <summary>
    /// Current time (in seconds) since the user has consumed nicotine.
    /// </summary>
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public float TimeSinceSmoking;

    /// <summary>
    /// EntityUid of the 'Chemicals' solution of the user.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public EntityUid ChemicalsContainer;
}



