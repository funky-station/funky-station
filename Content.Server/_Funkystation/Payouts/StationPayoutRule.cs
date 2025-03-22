using System.Linq;
using Content.Server.Cargo.Components;
using Content.Server.CriminalRecords.Systems;
using Content.Server.StationEvents.Events;
using Content.Server.StationRecords.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.StationRecords;

namespace Content.Server._Funkystation.Payouts;

public sealed class StationPayoutRule : StationEventSystem<StationPayoutRuleComponent>
{
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly CriminalRecordsSystem _criminalRecords = default!;

    protected override void Started(EntityUid uid, StationPayoutRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!TryGetRandomStation(out var chosenStation, HasComp<StationBankAccountComponent>))
            return;

        if (!TryComp<StationBankAccountComponent>(chosenStation.Value, out var bankAccountComponent))
            return;

        var recordEnumerable = _stationRecords.GetRecordsOfType<GeneralStationRecord>(chosenStation.Value);

        var valueTuples = recordEnumerable as (uint, GeneralStationRecord)[] ?? recordEnumerable.ToArray();
        foreach (var record in valueTuples)
        {
            // todo: people who are detained do not get paid, should also make that a HoP setting
        }

        // bankAccountComponent.ScripBalance +=
    }
}
