// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Arendian <137322659+Arendian@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Winkarst <74284083+Winkarst-cpu@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.UserInterface.Controls;
using Content.Shared.Access.Systems;
using Content.Shared.Administration;
using Content.Shared.CriminalRecords;
using Content.Shared.Dataset;
using Content.Shared.Random.Helpers;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Client.AutoGenerated;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using System.Linq;
using System.Numerics;
using Content.Shared.StatusIcon;
using Robust.Client.GameObjects;

namespace Content.Client.CriminalRecords;

// TODO: dedupe shitcode from general records theres a lot
[GenerateTypedNameReferences]
public sealed partial class CriminalRecordsConsoleWindow : FancyWindow
{
    private readonly IPlayerManager _player;
    private readonly IPrototypeManager _proto;
    private readonly IRobustRandom _random;
    private readonly AccessReaderSystem _accessReader;
    [Dependency] private readonly IEntityManager _entManager = default!;
    private readonly SpriteSystem _spriteSystem;

    public readonly EntityUid Console;

    [ValidatePrototypeId<LocalizedDatasetPrototype>]
    private const string ReasonPlaceholders = "CriminalRecordsWantedReasonPlaceholders";

    public Action<uint?>? OnKeySelected;
    public Action<StationRecordFilterType, string>? OnFiltersChanged;
    public Action<SecurityStatus>? OnStatusSelected;
    public Action<uint>? OnCheckStatus;
    public Action<CriminalRecord, bool, bool>? OnHistoryUpdated;
    public Action? OnHistoryClosed;
    public Action<SecurityStatus, string>? OnDialogConfirmed;

    public Action<SecurityStatus>? OnStatusFilterPressed;
    private uint _maxLength;
    private bool _access;
    private uint? _selectedKey;
    private CriminalRecord? _selectedRecord;

    private DialogWindow? _reasonDialog;

    private StationRecordFilterType _currentFilterType;

    private SecurityStatus _currentCrewListFilter;

    public CriminalRecordsConsoleWindow(EntityUid console, uint maxLength, IPlayerManager playerManager, IPrototypeManager prototypeManager, IRobustRandom robustRandom, AccessReaderSystem accessReader)
    {
        RobustXamlLoader.Load(this);

        Console = console;
        _player = playerManager;
        _proto = prototypeManager;
        _random = robustRandom;
        _accessReader = accessReader;
        IoCManager.InjectDependencies(this);
        _spriteSystem = _entManager.System<SpriteSystem>();

        _maxLength = maxLength;
        _currentFilterType = StationRecordFilterType.Name;

        _currentCrewListFilter = SecurityStatus.None;

        OpenCentered();

        foreach (var item in Enum.GetValues<StationRecordFilterType>())
        {
            FilterType.AddItem(GetTypeFilterLocals(item), (int)item);
        }

        foreach (var status in Enum.GetValues<SecurityStatus>())
        {
            AddStatusSelect(status);
        }

        //Populate status to filter crew list
        foreach (var item in Enum.GetValues<SecurityStatus>())
        {
            CrewListFilter.AddItem(GetCrewListFilterLocals(item), (int)item);
        }

        OnClose += () => _reasonDialog?.Close();

        RecordListing.OnItemSelected += args =>
        {
            if (RecordListing[args.ItemIndex].Metadata is not uint cast)
                return;

            OnKeySelected?.Invoke(cast);
        };

        RecordListing.OnItemDeselected += _ =>
        {
            OnKeySelected?.Invoke(null);
        };

        FilterType.OnItemSelected += eventArgs =>
        {
            var type = (StationRecordFilterType)eventArgs.Id;

            if (_currentFilterType != type)
            {
                _currentFilterType = type;
                FilterListingOfRecords(FilterText.Text);
            }
        };

        //Select Status to filter crew
        CrewListFilter.OnItemSelected += eventArgs =>
        {
            var type = (SecurityStatus)eventArgs.Id;

            if (_currentCrewListFilter != type)
            {
                _currentCrewListFilter = type;

                StatusFilterPressed(type);

            }
        };

        FilterText.OnTextEntered += args =>
        {
            FilterListingOfRecords(args.Text);
        };

        StatusOptionButton.OnItemSelected += args =>
        {
            SetStatus((SecurityStatus)args.Id);
        };

        HistoryButton.OnPressed += _ =>
        {
            if (_selectedRecord is { } record)
                OnHistoryUpdated?.Invoke(record, _access, true);
        };
    }

    public void StatusFilterPressed(SecurityStatus statusSelected)
    {
        OnStatusFilterPressed?.Invoke(statusSelected);
    }

    public void UpdateState(CriminalRecordsConsoleState state)
    {
        if (state.Filter != null)
        {
            if (state.Filter.Type != _currentFilterType)
            {
                _currentFilterType = state.Filter.Type;
            }

            if (state.Filter.Value != FilterText.Text)
            {
                FilterText.Text = state.Filter.Value;
            }
        }

        if (state.FilterStatus != _currentCrewListFilter)
        {
            _currentCrewListFilter = state.FilterStatus;
        }

        _selectedKey = state.SelectedKey;
        FilterType.SelectId((int)_currentFilterType);
        CrewListFilter.SelectId((int)_currentCrewListFilter);
        NoRecords.Visible = state.RecordListing == null || state.RecordListing.Count == 0;
        PopulateRecordListing(state.RecordListing);

        // set up the selected person's record
        var selected = _selectedKey != null;

        PersonContainer.Visible = selected;
        RecordUnselected.Visible = !selected;

        _access = _player.LocalSession?.AttachedEntity is {} player
            && _accessReader.IsAllowed(player, Console);

        // hide access-required editing parts when no access
        var editing = _access && selected;
        StatusOptionButton.Disabled = !editing;

        if (state is { CriminalRecord: not null, StationRecord: not null })
        {
            PopulateRecordContainer(state.StationRecord, state.CriminalRecord);
            OnHistoryUpdated?.Invoke(state.CriminalRecord, _access, false);
            _selectedRecord = state.CriminalRecord;
        }
        else
        {
            _selectedRecord = null;
            OnHistoryClosed?.Invoke();
        }
    }

    private void PopulateRecordListing(Dictionary<uint, string>? listing)
    {
        if (listing == null)
        {
            RecordListing.Clear();
            return;
        }

        var entries = listing.ToList();
        entries.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.Ordinal));
        // `entries` now contains the definitive list of items which should be in
        // our list of records and is in the order we want to present those items.

        // Walk through the existing items in RecordListing and in the updated listing
        // in parallel to synchronize the items in RecordListing with `entries`.
        int i = RecordListing.Count - 1;
        int j = entries.Count - 1;
        while (i >= 0 && j >= 0)
        {
            var strcmp = string.Compare(RecordListing[i].Text, entries[j].Value, StringComparison.Ordinal);
            if (strcmp == 0)
            {
                // This item exists in both RecordListing and `entries`. Nothing to do.
                i--;
                j--;
            }
            else if (strcmp > 0)
            {
                // Item exists in RecordListing, but not in `entries`. Remove it.
                RecordListing.RemoveAt(i);
                i--;
            }
            else if (strcmp < 0)
            {
                // A new entry which doesn't exist in RecordListing. Create it.
                RecordListing.Insert(i + 1, new ItemList.Item(RecordListing){Text = entries[j].Value, Metadata = entries[j].Key});
                j--;
            }
        }

        // Any remaining items in RecordListing don't exist in `entries`, so remove them
        while (i >= 0)
        {
            RecordListing.RemoveAt(i);
            i--;
        }

        // And finally, any remaining items in `entries`, don't exist in RecordListing. Create them.
        while (j >= 0)
        {
            RecordListing.Insert(0, new ItemList.Item(RecordListing){ Text = entries[j].Value, Metadata = entries[j].Key });
            j--;
        }
    }
    private void PopulateRecordContainer(GeneralStationRecord stationRecord, CriminalRecord criminalRecord)
    {
        var specifier = new SpriteSpecifier.Rsi(new ResPath("Interface/Misc/job_icons.rsi"), "Unknown");
        var na = Loc.GetString("generic-not-available-shorthand");
        PersonName.Text = stationRecord.Name;
        PersonJob.Text = stationRecord.JobTitle ?? na;

        // Job icon
        if (_proto.TryIndex<JobIconPrototype>(stationRecord.JobIcon, out var proto))
        {
            PersonJobIcon.Texture = _spriteSystem.Frame0(proto.Icon);
        }

        PersonPrints.Text = stationRecord.Fingerprint ??  Loc.GetString("generic-not-available-shorthand");
        PersonDna.Text = stationRecord.DNA ??  Loc.GetString("generic-not-available-shorthand");

        if (criminalRecord.Status != SecurityStatus.None)
        {
            specifier = new SpriteSpecifier.Rsi(new ResPath("Interface/Misc/security_icons.rsi"),  GetStatusIcon(criminalRecord.Status));
        }
        PersonStatusTX.SetFromSpriteSpecifier(specifier);
        PersonStatusTX.DisplayRect.TextureScale = new Vector2(3f, 3f);

        StatusOptionButton.SelectId((int)criminalRecord.Status);
        if (criminalRecord.Reason is { } reason)
        {
            var message = FormattedMessage.FromMarkupOrThrow(Loc.GetString("criminal-records-console-wanted-reason"));

            if (criminalRecord.Status == SecurityStatus.Suspected)
            {
                message = FormattedMessage.FromMarkupOrThrow(Loc.GetString("criminal-records-console-suspected-reason"));
            }
            message.AddText($": {reason}");

            WantedReason.SetMessage(message);
            WantedReason.Visible = true;
        }
        else
        {
            WantedReason.Visible = false;
        }
    }

    private void AddStatusSelect(SecurityStatus status)
    {
        var name = Loc.GetString($"criminal-records-status-{status.ToString().ToLower()}");
        StatusOptionButton.AddItem(name, (int)status);
    }

    private void FilterListingOfRecords(string text = "")
    {
        OnFiltersChanged?.Invoke(_currentFilterType, text);
    }

    private void SetStatus(SecurityStatus status)
    {
        if (status == SecurityStatus.Wanted || status == SecurityStatus.Suspected)
        {
            GetReason(status);
            return;
        }

        OnStatusSelected?.Invoke(status);
    }

    private void GetReason(SecurityStatus status)
    {
        if (_reasonDialog != null)
        {
            _reasonDialog.MoveToFront();
            return;
        }

        var field = "reason";
        var title = Loc.GetString("criminal-records-status-" + status.ToString().ToLower());
        var placeholders = _proto.Index<LocalizedDatasetPrototype>(ReasonPlaceholders);
        var placeholder = Loc.GetString("criminal-records-console-reason-placeholder", ("placeholder", _random.Pick(placeholders))); // just funny it doesn't actually get used
        var prompt = Loc.GetString("criminal-records-console-reason");
        var entry = new QuickDialogEntry(field, QuickDialogEntryType.LongText, prompt, placeholder);
        var entries = new List<QuickDialogEntry>() { entry };
        _reasonDialog = new DialogWindow(title, entries);

        _reasonDialog.OnConfirmed += responses =>
        {
            var reason = responses[field];
            if (reason.Length < 1 || reason.Length > _maxLength)
                return;

            OnDialogConfirmed?.Invoke(status, reason);
        };

        _reasonDialog.OnClose += () => { _reasonDialog = null; };
    }
    private string GetStatusIcon(SecurityStatus status)
    {
        return status switch
        {
            SecurityStatus.Paroled => "hud_paroled",
            SecurityStatus.Wanted => "hud_wanted",
            SecurityStatus.Detained => "hud_incarcerated",
            SecurityStatus.Discharged => "hud_discharged",
            SecurityStatus.Suspected => "hud_suspected",
            _ => "SecurityIconNone"
        };
    }
    private string GetTypeFilterLocals(StationRecordFilterType type)
    {
        return Loc.GetString($"criminal-records-{type.ToString().ToLower()}-filter");
    }

    private string GetCrewListFilterLocals(SecurityStatus type)
    {
        string result;

        // If "NONE" override to "show all"
        if (type == SecurityStatus.None)
        {
            result = Loc.GetString("criminal-records-console-show-all");
        }
        else
        {
            result = Loc.GetString($"criminal-records-status-{type.ToString().ToLower()}");
        }

        return result;
    }
}
