using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.Chat.Systems;
using Robust.Shared.Timing;
using Content.Server.GameTicking.Rules;
using Content.Server.StationRecords.Systems;
using Content.Shared.CriminalRecords;
using Content.Shared.GameTicking.Components;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Content.Server._Funkystation.Payouts.Components;
using Content.Shared.Cargo.Components;

namespace Content.Server._Funkystation.Payouts;

public sealed class StationRegularPayoutSystem : GameRuleSystem<StationRegularPayoutComponent>
{
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly ScripSystem _scrip = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PayoutSystem _payoutSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        // this is where I would put my subscriptions... IF I HAD ANY
    }

    protected override void Added(EntityUid uid, StationRegularPayoutComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<StationRegularPayoutComponent>(uid, out var payoutComponent))
            return;

        payoutComponent.NextPayout = TimeSpan.FromMinutes(payoutComponent.RegularPayoutInterval);
    }

    protected override void Started(EntityUid uid, StationRegularPayoutComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryComp<StationRegularPayoutComponent>(uid, out var payoutComponent))
            return;

        // grab a station, there should only be one valid station
        if (!TryGetRandomStation(out var chosenStation, HasComp<StationBankAccountComponent>))
            return;
        component.StationComponentUID = chosenStation.Value;

        InitiatePayouts(payoutComponent);
    }

    private void InitiatePayouts(StationRegularPayoutComponent component)
    {
        // use our cached value, if it no longer exists because shenanigans, fetch a new valid station.
        if (!TryComp<StationBankAccountComponent>(component.StationComponentUID, out var bankAccountComponent))
            return;

        var recordEnumerable = _stationRecords.GetRecordsOfType<CriminalRecord>(component.StationComponentUID);

        var valueTuples = recordEnumerable as (uint, CriminalRecord)[] ?? recordEnumerable.ToArray();

        var validCrew = 0;
        foreach (var criminalRecord in valueTuples)
        {
            var key = new StationRecordKey(criminalRecord.Item1, component.StationComponentUID);
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

        _scrip.AddScripToStation(bankAccountComponent, totalStationPayoutAmount);

        if (!component.ScripInitialStationInit)
        {
            component.ScripInitialStationInit = true;
            PayoutStation(component.StationComponentUID, bankAccountComponent, false);

            return;
        }

        PayoutStation(component.StationComponentUID, bankAccountComponent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<StationRegularPayoutComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var payoutComponent, out var ruleData))
        {
            if (!GameTicker.IsGameRuleAdded(uid, ruleData))
                continue;

            if (!GameTicker.IsGameRuleActive(uid, ruleData) && !HasComp<DelayedStartRuleComponent>(uid))
            {
                GameTicker.StartGameRule(uid, ruleData);
            }

            /**
            * only do the full station payout if its been long enough.
            */
            if (payoutComponent.NextPayout > _gameTiming.CurTime)
            {
                continue;
            }

            InitiatePayouts(payoutComponent);
            payoutComponent.NextPayout += TimeSpan.FromMinutes(payoutComponent.RegularPayoutInterval);
        }
    }

    private void PayoutStation(EntityUid stationUid, StationBankAccountComponent stationBankAccount, bool announceToStation = true)
    {
        var paymentRecords = _stationRecords.GetRecordsOfType<PaymentRecord>(stationUid);

        foreach (var paymentRecord in paymentRecords)
        {
            var history = new PayoutReceipt();

            if (paymentRecord.Item2.PaySuspended)
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

            _payoutSystem.PayOutToBalance(record, paymentRecord.Item2.PayoutAmount, stationBankAccount);
        }

        Log.Info("Station paid out through PayoutStation");

        if (announceToStation)
            _chatSystem.DispatchStationAnnouncement(stationUid, Loc.GetString("scrip-scheduled-payout"));
    }
}
