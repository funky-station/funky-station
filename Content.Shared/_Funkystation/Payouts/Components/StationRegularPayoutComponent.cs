namespace Content.Server._Funkystation.Payouts.Components;

[RegisterComponent]
public sealed partial class StationRegularPayoutComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripBaseStationBalanceAdd = 300_000;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripPerPlayerBalanceAdd = 3000;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ScripInitialStationInit = false;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int RegularPayoutInterval = 10;

    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan NextPayout = TimeSpan.FromMinutes(10);

    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntityUid StationComponentUID;
}
