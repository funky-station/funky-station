using System.Linq;
using Content.Server.GameTicking.Rules;
using Content.Server.StationRecords.Systems;
using Content.Shared._Funkystation.Economy.Components;
using Content.Shared._Funkystation.Economy.StationRecords;
using Content.Shared.Cargo.Components;
using Content.Shared.CriminalRecords;
using Content.Shared.GameTicking.Components;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Economy;

public sealed class StationRegularPayoutSystem : GameRuleSystem<StationRegularPayoutComponent>
{
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PayoutSystem _payoutSystem = default!;

    protected override void Added(EntityUid uid, StationRegularPayoutComponent component, GameRuleComponent gameRule,
        GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<StationRegularPayoutComponent>(uid, out var payoutComponent))
            return;

        payoutComponent.NextPayout = TimeSpan.FromMinutes(payoutComponent.RegularPayoutInterval);
    }

    protected override void Started(EntityUid uid, StationRegularPayoutComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryComp<StationRegularPayoutComponent>(uid, out var payoutComponent))
            return;

        // grab a station, there should only be one valid station
        if (!TryGetRandomStation(out var chosenStation, HasComp<StationBankAccountComponent>))
            return;

        component.StationComponentUid = chosenStation.Value;

        InitiatePayouts(payoutComponent);
    }

    private void InitiatePayouts(StationRegularPayoutComponent component)
    {
        // use our cached value, if it no longer exists because shenanigans, fetch a new valid station.
        if (!TryComp<StationBankAccountComponent>(component.StationComponentUid, out var bankAccountComponent))
            return;

        var recordEnumerable = _stationRecords.GetRecordsOfType<CriminalRecord>(component.StationComponentUid);

        var valueTuples = recordEnumerable as (uint, CriminalRecord)[] ?? recordEnumerable.ToArray();

        var validCrew = 0;
        foreach (var criminalRecord in valueTuples)
        {
            var key = new StationRecordKey(criminalRecord.Item1, component.StationComponentUid);
            _stationRecords.TryGetRecord(key, out PaymentRecord? record);

            if (record is null)
                continue;

            if (criminalRecord.Item2.Status.Id is nameof(SecurityStatus.Detained) or nameof(SecurityStatus.Wanted))
            {
                record.PaySuspended = true;
                continue;
            }

            record.PaySuspended = false;
            validCrew++; // only pay the station for crew that is nice :)
        }

        var totalStationPayoutAmount =
            component.ScripBaseStationBalanceAdd + (component.ScripPerPlayerBalanceAdd * validCrew);

        _payoutSystem.AddScripToStation(bankAccountComponent, totalStationPayoutAmount);

        if (!component.ScripInitialStationInit)
        {
            component.ScripInitialStationInit = true;
            PayoutStation(component.StationComponentUid, bankAccountComponent, false);

            return;
        }

        PayoutStation(component.StationComponentUid, bankAccountComponent);
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

            /*
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

    private void PayoutStation(EntityUid stationUid, StationBankAccountComponent stationBankAccount,
        bool announceToStation = true)
    {
        _payoutSystem.PayoutStation(stationUid, stationBankAccount, announceToStation);
    }
}