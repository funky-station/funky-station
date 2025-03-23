using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.CriminalRecords.Systems;
using Content.Server.GameTicking;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.StationRecords.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Payouts;

public sealed class StationPayoutRule : StationEventSystem<StationPayoutRuleComponent>
{
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly PayoutSystem _payoutSystem = default!;

    protected override void Started(EntityUid uid, StationPayoutRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation, HasComp<StationBankAccountComponent>))
            return;

        if (!TryComp<StationBankAccountComponent>(chosenStation.Value, out var bankAccountComponent))
            return;

        var recordEnumerable = _stationRecords.GetRecordsOfType<CriminalRecord>(chosenStation.Value);

        var valueTuples = recordEnumerable as (uint, CriminalRecord)[] ?? recordEnumerable.ToArray();

        var validCrew = 0;
        foreach (var criminalRecord in valueTuples)
        {
            var key = new StationRecordKey(criminalRecord.Item1, chosenStation.Value);
            _stationRecords.TryGetRecord(key, out PaymentRecord? record);

            if (record is null)
                continue;

            if (criminalRecord.Item2.Status is SecurityStatus.Detained or SecurityStatus.Wanted)
            {
                record.PaySuspended = true;
                continue;
            }

            record.PaySuspended = false;
            validCrew++; // only pay the station for crew that is nice :)
        }

        var totalStationPayoutAmount =
            component.ScripBaseStationBalanceAdd + (component.ScripPerPlayerBalanceAdd * validCrew);

        bankAccountComponent.ScripBalance += totalStationPayoutAmount;

        PayoutStation(chosenStation.Value, bankAccountComponent);
    }

    private void PayoutStation(EntityUid stationUid, StationBankAccountComponent stationBankAccount)
    {
        var paymentRecords = _stationRecords.GetRecordsOfType<PaymentRecord>(stationUid);

        foreach (var paymentRecord in paymentRecords)
        {
            var history = new PayoutReceipt();

            if (paymentRecord.Item2.PaySuspended)
            {
                history.Paid = false;
            }

            history.PayoutTime = _ticker.RoundStartTimeSpan;
            history.Amount = paymentRecord.Item2.PayoutAmount;

            paymentRecord.Item2.History.Add(history);

            if (!history.Paid)
                continue;

            var key = new StationRecordKey(paymentRecord.Item1, stationUid);
            _stationRecords.TryGetRecord(key, out GeneralStationRecord? record);

            if (record is null)
                continue;

            _payoutSystem.PayOutToBalance(record, paymentRecord.Item2.PayoutAmount, stationBankAccount);
        }
    }
}
