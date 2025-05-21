using Content.Server._Funkystation.Payouts.Prototypes;
using Content.Server.Cargo.Components;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Server.StationRecords.Systems;
using Content.Shared._Funkystation.CCVars;
using Content.Shared._Funkystation.Payouts;
using Content.Shared.Cargo.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Payouts;

public sealed class PayoutSystem : EntitySystem
{
    [Dependency] private readonly ScripSystem _scrip = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;

    private readonly Dictionary<EntityUid, PaymentRecord> _cachedEntries = new();
    public readonly Dictionary<ProtoId<JobPrototype>, int> InitialPayoutInfo = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(AfterGeneralRecordCreated);

        PopulateSalaryForAllJobs();
    }

    private void AfterGeneralRecordCreated(AfterGeneralRecordCreatedEvent ev)
    {
        GeneratePaymentRecordEntry(ev.Station, ev.Key, ev.Profile);
    }

    private void GeneratePaymentRecordEntry(EntityUid station, StationRecordKey stationRecordKey, HumanoidCharacterProfile profile)
    {

    }

    private void PopulateSalaryForAllJobs()
    {
        var allSalaryInfo = _prototypeManager.EnumeratePrototypes<PaymentSalaryPrototype>();

        foreach (var salaryInfo in allSalaryInfo)
        {
            foreach (var job in salaryInfo.Roles)
            {
                InitialPayoutInfo.TryAdd(job, salaryInfo.Salary);
            }
        }
    }

    public bool PayOutToBalance(GeneralStationRecord record, int amount, StationBankAccountComponent stationBankAccount)
    {
        if (_config.GetCVar(CCVars_Funky.EnablePersistentBalance))
        {
            throw new NotImplementedException();
            // PayOutToCharacterBalance(characterUid, amount);
            return true;
        }
        
        record.Balance += amount;
        _scrip.RemoveScripFromStation(stationBankAccount, amount);

        return true;
    }

    // for forks that want persistent money
    private bool PayOutToCharacterBalance(EntityUid characterUid, int amount)
    {
        throw new NotImplementedException();
    }
}