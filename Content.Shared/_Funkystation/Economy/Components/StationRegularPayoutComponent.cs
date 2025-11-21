namespace Content.Shared._Funkystation.Economy.Components;

[RegisterComponent]
public sealed partial class StationRegularPayoutComponent : Component
{
    [DataField] public int ScripBaseStationBalanceAdd = 300_000;
    [DataField] public int ScripPerPlayerBalanceAdd = 3000;
    [DataField] public bool ScripInitialStationInit = false;
    [DataField] public int RegularPayoutInterval = 10;
    [DataField] public TimeSpan NextPayout = TimeSpan.FromMinutes(10);
    
    [ViewVariables(VVAccess.ReadOnly), DataField]
    public EntityUid StationComponentUid;
}