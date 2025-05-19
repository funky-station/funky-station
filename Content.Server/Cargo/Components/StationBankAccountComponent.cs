using Content.Server._Funkystation.Payouts;
using Content.Shared.Cargo;

namespace Content.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track its money.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem), typeof(StationRegularPayoutSystem), typeof(PayoutSystem))]
public sealed partial class StationBankAccountComponent : Component
{
    /// <summary>
    /// funky station: company scrip
    /// starter station scrip balance
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public int ScripBalance = 60_000;

    /// <summary>
    /// funky station: company scrip
    /// time between payouts
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public TimeSpan ScripStationPayoutPeriod = TimeSpan.FromMinutes(10);

    /// <summary>
    /// funky station: company scrip
    /// payout people who are detained or wanted
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public bool ScripPayoutDetainedOrWanted = false;
}
