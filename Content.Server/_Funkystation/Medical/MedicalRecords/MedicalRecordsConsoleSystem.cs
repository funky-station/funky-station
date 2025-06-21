using Content.Server._Funkystation.Records;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Server.StationRecords;
using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.CriminalRecords;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Content.Shared._Funkystation.Records;
using Robust.Server.GameObjects;

namespace Content.Server._Funkystation.Medical.MedicalRecords;

public sealed class MedicalRecordsConsoleSystem : EntitySystem
{
    [Dependency] private readonly CharacterRecordsSystem _characterRecords = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MedicalRecordsConsoleComponent, CharacterRecordsModifiedEvent>((uid, component, _) =>
            UpdateUi(uid, component));

        Subs.BuiEvents<MedicalRecordsConsoleComponent>(MedicalRecordsConsoleKey.Key,
            subr =>
            {
                subr.Event<BoundUIOpenedEvent>((uid, component, _) => UpdateUi(uid, component));
                subr.Event<MedicalRecordsConsoleSelectMsg>(OnKeySelect);
                subr.Event<MedicalRecordsConsoleFilterMsg>(OnFilterApplied);
            });
    }

    private void OnFilterApplied(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordsConsoleFilterMsg msg)
    {
        ent.Comp.Filter = msg.Filter;
        UpdateUi(ent);
    }

    private void OnKeySelect(Entity<MedicalRecordsConsoleComponent> ent, ref MedicalRecordsConsoleSelectMsg msg)
    {
        ent.Comp.SelectedIndex = msg.CharacterRecordKey;
        UpdateUi(ent);
    }

    private void UpdateUi(EntityUid entity, MedicalRecordsConsoleComponent? console = null)
    {
        if (!Resolve(entity, ref console))
            return;

        var station = _station.GetOwningStation(entity);
        if (!HasComp<StationRecordsComponent>(station) || !HasComp<CharacterRecordsComponent>(station))
            return;

        var characterRecords = _characterRecords.QueryRecords(station.Value);
        // Get the name and station records key display from the list of records
        var names = new Dictionary<uint, MedicalRecordsConsoleState.CharacterInfo>();
        foreach (var (i, r) in characterRecords)
        {
            var nameJob = $"{r.Name} ({r.JobTitle})";

            // Apply any filter the user has set
            if (console.Filter != null)
            {
                if (IsSkippedRecord(console.Filter, r, nameJob))
                    continue;
            }

            if (names.ContainsKey(i))
            {
                Log.Error(
                    $"We somehow have duplicate character record keys, NetEntity: {i}, Entity: {entity}, Character Name: {r.Name}");
            }

            names[i] = new MedicalRecordsConsoleState.CharacterInfo
                { CharacterDisplayName = nameJob, StationRecordKey = r.StationRecordsKey };
        }

        var record =
            console.SelectedIndex == null || !characterRecords.TryGetValue(console.SelectedIndex!.Value, out var value)
                ? null
                : value;

        SendState(entity,
            new MedicalRecordsConsoleState
            {
                CharacterList = names,
                SelectedIndex = console.SelectedIndex,
                SelectedRecord = record,
                Filter = console.Filter,
            });
    }

    private void SendState(EntityUid entity, MedicalRecordsConsoleState state)
    {
        _ui.SetUiState(entity, MedicalRecordsConsoleKey.Key, state);
    }

    /// <summary>
    /// Almost exactly the same as <see cref="StationRecordsSystem.IsSkipped"/>
    /// </summary>
    private static bool IsSkippedRecord(StationRecordsFilter filter,
        FullCharacterRecords record,
        string nameJob)
    {
        var isFilter = filter.Value.Length > 0;

        if (!isFilter)
            return false;

        var filterLowerCaseValue = filter.Value.ToLower();

        return filter.Type switch
        {
            StationRecordFilterType.Name =>
                !nameJob.Contains(filterLowerCaseValue, StringComparison.CurrentCultureIgnoreCase),
            StationRecordFilterType.Prints => record.Fingerprint != null
                && IsFilterWithSomeCodeValue(record.Fingerprint, filterLowerCaseValue),
            StationRecordFilterType.DNA => record.DNA != null
                                                && IsFilterWithSomeCodeValue(record.DNA, filterLowerCaseValue),
            _ => throw new ArgumentOutOfRangeException(nameof(filter), "Invalid Character Record filter type"),
        };
    }

    private static bool IsFilterWithSomeCodeValue(string value, string filter)
    {
        return !value.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase);
    }
}
