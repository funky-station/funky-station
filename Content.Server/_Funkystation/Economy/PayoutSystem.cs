using Content.Server.Chat.Systems;
using Content.Server.GameTicking;
using Content.Server.StationRecords.Systems;
using Content.Shared._Funkystation.Economy;
using Content.Shared._Funkystation.Economy.StationRecords;
using Content.Shared.Cargo.Components;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.StationRecords;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Economy;

public sealed partial class PayoutSystem : SharedPayoutSystem
{
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    private readonly Dictionary<EntityUid, PaymentRecord> _cachedEntries = new();
    public readonly Dictionary<ProtoId<JobPrototype>, int> InitialPayoutInfo = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<AfterGeneralRecordCreatedEvent>(AfterGeneralRecordCreated);

        PopulateSalaryForAllJobs();
    }

    private void AfterGeneralRecordCreated(AfterGeneralRecordCreatedEvent ev)
    {
        GeneratePaymentRecordEntry(ev.Station, ev.Key, ev.Profile, ev.Record);
    }

    private void GeneratePaymentRecordEntry(EntityUid station, StationRecordKey stationRecordKey, HumanoidCharacterProfile profile, GeneralStationRecord generalStationRecord)
    {
        var recordId = _stationRecords.GetRecordByName(station, profile.Name);

        if (recordId == null)
            return;

        var key = new StationRecordKey((uint) recordId, station);

        _stationRecords.AddRecordEntry(stationRecordKey, new PaymentRecord());
        _stationRecords.Synchronize(stationRecordKey);

        if (!_stationRecords.TryGetRecord(key, out PaymentRecord? record))
            return;

        record.PayoutAmount = InitialPayoutInfo[(ProtoId<JobPrototype>) generalStationRecord.JobPrototype];
    }

    private void PopulateSalaryForAllJobs()
    {
        var allSalaryInfo = _prototypeManager.EnumeratePrototypes<PaymentSalaryPrototype>();

        foreach (var salaryInfo in allSalaryInfo)
        {
            foreach (var job in salaryInfo.Roles)
            {
                if(InitialPayoutInfo.TryGetValue(job, out var salary))
                {
                    InitialPayoutInfo[job] = salary + salaryInfo.Salary;
                    continue;
                }

                InitialPayoutInfo.TryAdd(job, salaryInfo.Salary);
            }
        }
    }

    public void PayoutStation(EntityUid stationUid, StationBankAccountComponent stationBankAccount, bool announceToStation = true)
    {
        var paymentRecords = _stationRecords.GetRecordsOfType<PaymentRecord>(stationUid);

        foreach (var paymentRecord in paymentRecords)
        {
            var history = new PayoutReceipt();

            history.Paid = true;

            if (paymentRecord.Item2.PaySuspended
                || stationBankAccount.ScripBalance < paymentRecord.Item2.PayoutAmount)
            {
                history.Paid = false;
            }

            history.PayoutTime = _gameTiming.CurTime;
            history.Amount = paymentRecord.Item2.PayoutAmount;

            paymentRecord.Item2.History.Add(history);

            if (!history.Paid)
                continue;

            var key = new StationRecordKey(paymentRecord.Item1, stationUid);
            _stationRecords.TryGetRecord(key, out GeneralStationRecord? record);

            if (record is null)
                continue;

            PayOutToBalance(record, paymentRecord.Item2.PayoutAmount, stationBankAccount);
        }

        Log.Info("Station paid out through PayoutStation");

        if (announceToStation)
            _chatSystem.DispatchStationAnnouncement(stationUid, Loc.GetString("scrip-scheduled-payout"));
    }

    public bool PayOutToBalance(GeneralStationRecord record, int amount, StationBankAccountComponent stationBankAccount)
    {
        record.Balance += amount;
        RemoveScripFromStation(stationBankAccount, amount);

        return true;
    }
}