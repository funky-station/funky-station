using Content.Shared._Funkystation.Payouts;
using Content.Shared.Cargo.Components;

namespace Content.Server._Funkystation.Payouts;

public sealed class ScripSystem : SharedScripSystem
{
    public override void Initialize()
    {
        base.Initialize();
        // this is where I would put my subscriptions... IF I HAD ANY
    }

    public void AddScripToStation(StationBankAccountComponent stationAccount, int amount)
    {
        stationAccount.ScripBalance += amount;
    }

    public void RemoveScripFromStation(StationBankAccountComponent stationAccount, int amount)
    {
        stationAccount.ScripBalance -= amount;
    }

    public void SetStationScrip(StationBankAccountComponent stationAccount, int amount)
    {
        stationAccount.ScripBalance = amount;
    }
}
