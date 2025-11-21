using Content.Shared.Cargo.Components;
using Content.Shared.StationRecords;

namespace Content.Shared._Funkystation.Economy;

public abstract class SharedPayoutSystem : EntitySystem
{
    [Dependency] private readonly SharedStationRecordsSystem _records = default!;
    
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