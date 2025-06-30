using System.Linq;
using Content.Server.Station.Systems;
using Content.Server.StationRecords;
using Content.Server.StationRecords.Systems;
using Content.Shared._Funkystation.Payouts;
using Content.Shared.CriminalRecords;
using Content.Shared.StationRecords;
using Robust.Server.GameObjects;

namespace Content.Server._Funkystation.Payouts;

public sealed class PayoutRecordsConsoleSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<PaymentRecordsConsoleComponent, RecordModifiedEvent>(UpdateUserInterface);
        SubscribeLocalEvent<PaymentRecordsConsoleComponent, AfterGeneralRecordCreatedEvent>(UpdateUserInterface);
        
        Subs.BuiEvents<PaymentRecordsConsoleComponent>(PaymentRecordsConsoleKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(UpdateUserInterface);
            subs.Event<SelectStationRecord>(OnKeySelected);
        });
    }

    private void UpdateUserInterface<T>(Entity<PaymentRecordsConsoleComponent> ent, ref T args)
    {
        // TODO: this is probably wasteful, maybe better to send a message to modify the exact state?
        UpdateUserInterface(ent);
    }
    
    private void OnKeySelected(Entity<PaymentRecordsConsoleComponent> ent, ref SelectStationRecord msg)
    {
        // no concern of sus client since record retrieval will fail if invalid id is given
        ent.Comp.ActiveKey = msg.SelectedKey;
        UpdateUserInterface(ent);
    }

    private void UpdateUserInterface(Entity<PaymentRecordsConsoleComponent> ent)
    {
        var (uid, comp) = ent;
        var owningStation = _station.GetOwningStation(uid);
        
        if (!TryComp<StationRecordsComponent>(owningStation, out var stationRecords))
        {
            _ui.SetUiState(uid, CriminalRecordsConsoleKey.Key, new PayoutRecordsConsoleState(null));
            return;
        }

        var listing = _records.BuildListing((owningStation.Value, stationRecords), null);

        Dictionary<uint, string> editedListings = [];
        foreach (var listEntry in listing)
        {
            _records.TryGetRecord<PaymentRecord>(new StationRecordKey(listEntry.Key, owningStation.Value), out var record);

            if (record != null)
            {
                editedListings.Add(listEntry.Key, $"{listEntry.Value} { (record.PaySuspended ? 
                    Loc.GetString("payment-records-console-suspended") : Loc.GetString("payment-records-console-eligible") 
                    )}");
            }
        }
        
        var state = new PayoutRecordsConsoleState(editedListings);

        if (comp.ActiveKey is { } id)
        {
            var key = new StationRecordKey(id, owningStation.Value);
            _records.TryGetRecord(key, out state.StationRecord, stationRecords);
            _records.TryGetRecord(key, out state.PaymentRecord, stationRecords);
            state.SelectedKey = id;
        }
        
        _ui.SetUiState(uid, PaymentRecordsConsoleKey.Key, state);
    }

}