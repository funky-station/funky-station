namespace Content.Server._Funkystation.Payouts;

public sealed partial class StationPayoutRuleComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripBaseStationBalanceAdd = 300_000;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripPerPlayerBalanceAdd = 3_000;
}
