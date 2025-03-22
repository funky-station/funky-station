using Content.Shared.Cargo;

namespace Content.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track its money.
/// </summary>
[RegisterComponent, Access(typeof(SharedCargoSystem))]
public sealed partial class StationBankAccountComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("balance")]
    public int Balance = 2000;

    /// <summary>
    /// How much the bank balance goes up per second, every Delay period. Rounded down when multiplied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("increasePerSecond")]
    public int IncreasePerSecond = 1;

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
}
