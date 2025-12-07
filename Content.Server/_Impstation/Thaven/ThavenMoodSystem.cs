// SPDX-FileCopyrightText: 2025 ATDoop <bug@bug.bug>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Actions;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.Roles.Jobs;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using Content.Shared.Dataset;
using Content.Shared.Emag.Systems;
using Content.Shared.GameTicking;
using Content.Shared._Impstation.Thaven;
using Content.Shared._Impstation.Thaven.Components;
using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Shared._Impstation.CCVar;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Utility;

namespace Content.Server._Impstation.Thaven;

public sealed partial class ThavenMoodsSystem : SharedThavenMoodSystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly UserInterfaceSystem _bui = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!; // funky
    [Dependency] private readonly JobSystem _jobs = default!; // funky

    public IReadOnlyList<ThavenMood> SharedMoods => _sharedMoods.AsReadOnly();
    private readonly List<ThavenMood> _sharedMoods = new();


    [ValidatePrototypeId<DatasetPrototype>]
    private const string SharedDataset = "ThavenMoodsShared";

    [ValidatePrototypeId<DatasetPrototype>]
    private const string YesAndDataset = "ThavenMoodsYesAnd";

    [ValidatePrototypeId<DatasetPrototype>]
    private const string NoAndDataset = "ThavenMoodsNoAnd";

    [ValidatePrototypeId<DatasetPrototype>]
    private const string WildcardDataset = "ThavenMoodsWildcard";

    [ValidatePrototypeId<EntityPrototype>]
    private const string ActionViewMoods = "ActionViewMoods";

    [ValidatePrototypeId<WeightedRandomPrototype>]
    private const string RandomThavenMoodDataset = "RandomThavenMoodDataset";

    public override void Initialize()
    {
        base.Initialize();

        NewSharedMoods();

        SubscribeLocalEvent<ThavenMoodsBoundComponent, ComponentStartup>(OnThavenMoodInit);
        SubscribeLocalEvent<ThavenMoodsBoundComponent, ComponentShutdown>(OnThavenMoodShutdown);
        SubscribeLocalEvent<ThavenMoodsBoundComponent, ToggleMoodsScreenEvent>(OnToggleMoodsScreen);
        SubscribeLocalEvent<ThavenMoodsBoundComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnSpawnComplete); // funky
        SubscribeLocalEvent<RoundRestartCleanupEvent>((_) => NewSharedMoods());
    }

    private void NewSharedMoods()
    {
        _sharedMoods.Clear();
        for (int i = 0; i < _config.GetCVar(ImpCCVars.ThavenSharedMoodCount); i++)
            TryAddSharedMood();
    }

    public bool TryAddSharedMood(ThavenMood? mood = null, bool checkConflicts = true)
    {
        if (mood == null)
        {
            if (TryPick(SharedDataset, out var moodProto, _sharedMoods))
            {
                mood = RollMood(moodProto);
                checkConflicts = false; // TryPick has cleared this mood already
            }
            else
            {
                return false;
            }
        }

        if (checkConflicts && (GetConflicts(_sharedMoods).Contains(mood.ProtoId) || GetMoodProtoSet(_sharedMoods).Overlaps(mood.Conflicts)))
            return false;

        _sharedMoods.Add(mood);
        var enumerator = EntityManager.EntityQueryEnumerator<ThavenMoodsBoundComponent>();
        while (enumerator.MoveNext(out var ent, out var comp))
        {
            if (!comp.FollowsSharedMoods)
                continue;

            NotifyMoodChange((ent, comp));
        }

        return true;
    }

    private void OnBoundUIOpened(EntityUid uid, ThavenMoodsBoundComponent boundComponent, BoundUIOpenedEvent args)
    {
        UpdateBUIState(uid, boundComponent);
    }

    private void OnToggleMoodsScreen(EntityUid uid, ThavenMoodsBoundComponent boundComponent, ToggleMoodsScreenEvent args)
    {
        if (args.Handled || !TryComp<ActorComponent>(uid, out var actor))
            return;
        args.Handled = true;

        _bui.TryToggleUi(uid, ThavenMoodsUiKey.Key, actor.PlayerSession);
    }

    private bool TryPick(string datasetProto, [NotNullWhen(true)] out ThavenMoodPrototype? proto,
        IEnumerable<ThavenMood>? currentMoods = null,
        HashSet<string>? conflicts = null,
        string? department = null)
    {
        var dataset = _proto.Index<DatasetPrototype>(datasetProto);
        var choices = dataset.Values.ToList();

        if (currentMoods == null)
            currentMoods = new HashSet<ThavenMood>();
        if (conflicts == null)
            conflicts = GetConflicts(currentMoods);
        if (department == null)
            department = Loc.GetString("generic-unknown-title"); // assume department is unknown if it isn't given

        var currentMoodProtos = GetMoodProtoSet(currentMoods);

        while (choices.Count > 0)
        {
            var moodId = _random.PickAndTake(choices);
            if (conflicts.Contains(moodId))
                continue; // Skip proto if an existing mood conflicts with it

            var moodProto = _proto.Index<ThavenMoodPrototype>(moodId);
            if (moodProto.Conflicts.Overlaps(currentMoodProtos))
                continue; // Skip proto if it conflicts with an existing mood

            // begin funky
            var conflictingJobs = moodProto.JobConflicts;
            if (conflictingJobs.Contains(department))
                continue; // Skip proto if it conflicts with the entity's department
            // end funky

            proto = moodProto;
            return true;
        }

        proto = null;
        return false;
    }

    public void NotifyMoodChange(Entity<ThavenMoodsBoundComponent> ent)
    {
        if (!TryComp<ActorComponent>(ent.Owner, out var actor))
            return;

        if (ent.Comp.MoodsChangedSound != null)
            _audio.PlayGlobal(ent.Comp.MoodsChangedSound, actor.PlayerSession);

        var msg = Loc.GetString("thaven-moods-update-notify");
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", msg));
        _chatManager.ChatMessageToOne(ChatChannel.Server, msg, wrappedMessage, default, false, actor.PlayerSession.Channel, colorOverride: Color.Orange);
    }

    public void UpdateBUIState(EntityUid uid, ThavenMoodsBoundComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return;

        var state = new ThavenMoodsBuiState(comp.Moods, comp.FollowsSharedMoods ? _sharedMoods : []);
        _bui.SetUiState(uid, ThavenMoodsUiKey.Key, state);
    }

    public void AddMood(EntityUid uid, ThavenMood mood, ThavenMoodsBoundComponent? comp = null, bool notify = true)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Moods.Add(mood);

        if (notify)
            NotifyMoodChange((uid, comp));

        UpdateBUIState(uid, comp);
    }

    /// <summary>
    /// Creates a ThavenMood instance from the given ThavenMoodPrototype, and rolls
    /// its mood vars.
    /// </summary>
    public ThavenMood RollMood(ThavenMoodPrototype proto)
    {
        var mood = new ThavenMood()
        {
            ProtoId = proto.ID,
            MoodName = proto.MoodName,
            MoodDesc = proto.MoodDesc,
            Conflicts = proto.Conflicts,
        };

        var alreadyChosen = new HashSet<string>();

        foreach (var (name, datasetID) in proto.MoodVarDatasets)
        {
            var dataset = _proto.Index<DatasetPrototype>(datasetID);

            if (proto.AllowDuplicateMoodVars)
            {
                mood.MoodVars.Add(name, _random.Pick(dataset));
                continue;
            }

            var choices = dataset.Values.ToList();
            var foundChoice = false;
            while (choices.Count > 0)
            {
                var choice = _random.PickAndTake(choices);
                if (alreadyChosen.Contains(choice))
                    continue;

                mood.MoodVars.Add(name, choice);
                alreadyChosen.Add(choice);
                foundChoice = true;
                break;
            }

            if (!foundChoice)
            {
                Log.Warning($"Ran out of choices for moodvar \"{name}\" in \"{proto.ID}\"! Picking a duplicate...");
                mood.MoodVars.Add(name, _random.Pick(_proto.Index<DatasetPrototype>(dataset)));
            }
        }

        return mood;
    }

    /// <summary>
    /// Checks if the given mood prototype conflicts with the current moods, and
    /// adds the mood if it does not.
    /// </summary>
    public bool TryAddMood(EntityUid uid, ThavenMoodPrototype moodProto, ThavenMoodsBoundComponent? comp = null, bool allowConflict = false, bool notify = true)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (!allowConflict && GetConflicts(uid, comp).Contains(moodProto.ID))
            return false;

        AddMood(uid, RollMood(moodProto), comp, notify);
        return true;
    }

    public bool TryAddRandomMood(EntityUid uid, string datasetProto, ThavenMoodsBoundComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        if (TryPick(datasetProto, out var moodProto, GetActiveMoods(uid, comp), null, GetMindDepartment(uid))) // funky - check for job conflicts
        {
            AddMood(uid, RollMood(moodProto), comp);
            return true;
        }

        return false;
    }

    public bool TryAddRandomMood(EntityUid uid, ThavenMoodsBoundComponent? comp = null)
    {
        if (!Resolve(uid, ref comp))
            return false;

        var datasetProto = _proto.Index<WeightedRandomPrototype>(RandomThavenMoodDataset).Pick();

        return TryAddRandomMood(uid, datasetProto, comp);
    }

    public void SetMoods(EntityUid uid, IEnumerable<ThavenMood> moods, ThavenMoodsBoundComponent? comp = null, bool notify = true)
    {
        if (!Resolve(uid, ref comp))
            return;

        comp.Moods = moods.ToList();
        if (notify)
            NotifyMoodChange((uid, comp));

        UpdateBUIState(uid, comp);
    }

    public HashSet<string> GetConflicts(IEnumerable<ThavenMood> moods)
    {
        var conflicts = new HashSet<string>();

        foreach (var mood in moods)
        {
            conflicts.Add(mood.ProtoId); // Specific moods shouldn't be added twice
            conflicts.UnionWith(mood.Conflicts);
        }

        return conflicts;
    }

    /// <summary>
    /// lazily wipes all of a Thaven's moods. This leaves them mood-less.
    /// </summary>
    public void ClearMoods(Entity<ThavenMoodsBoundComponent> ent, bool notify = false)
    {
        ent.Comp.Moods = new List<ThavenMood>();
        Dirty(ent);

        if (notify)
            NotifyMoodChange(ent);
        else
            UpdateBUIState(ent);
    }


    /// <summary>
    /// Allows external systems to toggle wether or not a ThavenMoodsComponent follows the shared thaven mood.
    /// </summary>
    public void ToggleSharedMoods(Entity<ThavenMoodsBoundComponent> ent, bool notify = false)
    {
        if (!ent.Comp.FollowsSharedMoods)
            ent.Comp.FollowsSharedMoods = true;
        else
            ent.Comp.FollowsSharedMoods = false;
        Dirty(ent);

        if (notify)
            NotifyMoodChange(ent);
        else
            UpdateBUIState(ent);
    }

    /// <summary>
    /// Allows external sytems to toggle wether or not a ThavenMoodsComponent is emaggable.
    /// </summary>
    public void ToggleEmaggable(Entity<ThavenMoodsBoundComponent> ent)
    {
        if (!ent.Comp.CanBeEmagged)
            ent.Comp.CanBeEmagged = true;
        else
            ent.Comp.CanBeEmagged = false;
        Dirty(ent);
    }

    public HashSet<string> GetConflicts(EntityUid uid, ThavenMoodsBoundComponent? moods = null)
    {
        // TODO: Should probably cache this when moods get updated

        if (!Resolve(uid, ref moods))
            return new();

        var conflicts = GetConflicts(GetActiveMoods(uid, moods));

        return conflicts;
    }

    public HashSet<string> GetMoodProtoSet(IEnumerable<ThavenMood> moods)
    {
        var moodProtos = new HashSet<string>();
        foreach (var mood in moods)
            if (!string.IsNullOrEmpty(mood.ProtoId))
                moodProtos.Add(mood.ProtoId);
        return moodProtos;
    }

    /// <summary>
    /// Return a list of the moods that are affecting this entity.
    /// </summary>
    public List<ThavenMood> GetActiveMoods(EntityUid uid, ThavenMoodsBoundComponent? comp = null, bool includeShared = true)
    {
        if (!Resolve(uid, ref comp))
            return [];

        if (includeShared && comp.FollowsSharedMoods)
        {
            return new List<ThavenMood>(SharedMoods.Concat(comp.Moods));
        }
        else
        {
            return comp.Moods;
        }
    }

    private void OnThavenMoodInit(EntityUid uid, ThavenMoodsBoundComponent comp, ComponentStartup args)
    {
        if (comp.LifeStage != ComponentLifeStage.Starting)
            return;

        /* funky: roll moods after jobs roll
        // "Yes, and" moods
        if (TryPick(YesAndDataset, out var mood, GetActiveMoods(uid, comp), null, GetMindDepartment(uid))) // funky - check for job conflicts
            TryAddMood(uid, mood, comp, true, false);

        // "No, and" moods
        if (TryPick(NoAndDataset, out mood, GetActiveMoods(uid, comp), null, GetMindDepartment(uid))) // funky - check for job conflicts
            TryAddMood(uid, mood, comp, true, false);
        */

        comp.Action = _actions.AddAction(uid, ActionViewMoods);
    }

    private void OnThavenMoodShutdown(EntityUid uid, ThavenMoodsBoundComponent comp, ComponentShutdown args)
    {
        _actions.RemoveAction(uid, comp.Action);
    }

    protected override void OnEmagged(EntityUid uid, ThavenMoodsBoundComponent comp, ref GotEmaggedEvent args)
    {
        base.OnEmagged(uid, comp, ref args);

        if (!HasComp<MindShieldComponent>(uid)) // funky: dont emag mindshielded thavens
            TryAddRandomMood(uid, WildcardDataset, comp);
    }

    // Begin DeltaV: thaven mood upsets
    public void AddWildcardMood(Entity<ThavenMoodsBoundComponent> ent)
    {
        TryAddRandomMood(ent.Owner, WildcardDataset, ent.Comp);
    }
    // End DeltaV: thaven mood upsets

    // begin funky
    private string GetMindDepartment(EntityUid uid)
    {
        var unknown = Loc.GetString("generic-unknown-title"); // TryGetJob returns this, so use it in place of "none" or something

        if (!_mind.TryGetMind(uid, out var mindId, out _))
            return unknown; // no mind no job

        if (!_jobs.MindTryGetJobId(mindId, out var jobName) || jobName == null)
            return unknown;

        if (!_jobs.TryGetDepartment(jobName, out var departmentProto))
            return unknown;

        return departmentProto.ID;
    }

    /// <summary>
    /// Some moods conflict with certain jobs + components initialize before jobs are rolled,
    /// so moods are rolled after spawn complete rather than <see cref="OnThavenMoodInit">ComponentStartup</see>
    /// </summary>
    private void OnSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        // check if the spawned mob is a thaven
        if (!TryComp<ThavenMoodsBoundComponent>(args.Mob, out var comp))
            return;

        // double check there aren't already some moods
        if (comp.Moods.Count >= 2)
            return;

        // "Yes, and" moods
        if (TryPick(YesAndDataset, out var mood, GetActiveMoods(args.Mob, comp), null, GetMindDepartment(args.Mob)))
            TryAddMood(args.Mob, mood, comp, true, false);

        // "No, and" moods
        if (TryPick(NoAndDataset, out mood, GetActiveMoods(args.Mob, comp), null, GetMindDepartment(args.Mob)))
            TryAddMood(args.Mob, mood, comp, true, false);
    }
    // end funky
}
