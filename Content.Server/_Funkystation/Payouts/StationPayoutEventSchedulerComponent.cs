namespace Content.Server._Funkystation.Payouts;

[RegisterComponent]
public sealed partial class StationPayoutEventSchedulerComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripBaseStationBalanceAdd = 300_000;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripPerPlayerBalanceAdd = 3_000;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ScripInitialStationInit = false;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float ScripPayoutDelay = 20;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float ScripNextPayoutTime = 0;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float ScripPayoutAccumulator = 0;
}
