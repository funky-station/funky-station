// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2026 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.Medical.Components;
using Content.Server.Popups;
using Content.Server.Research.Components;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Content.Shared._Funkystation.Genetics.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DeviceLinking;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed class DnaScannerConsoleSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;
    [Dependency] private readonly GeneticShuffleSystem _shuffle = default!;
    [Dependency] private readonly SharedMutationDiscoverySystem _discovery = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly MutationUnlockTriggerSystem _unlockTrigger = default!;

    private const float SequencerButtonRadiationDamage = 0.2f;
    private const float ScrambleRadiationDamage = 15f;
    private const float ScrambleCooldownSeconds = 30f;
    private const int MaxActiveResearchSlots = 5;
    private const int ResearchDurationSeconds = 180;
    private const float JokerCooldownSeconds = 600f;
    private static readonly TimeSpan UpdateTickInterval = TimeSpan.FromSeconds(1);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaScannerConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DnaScannerConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<DnaScannerConsoleComponent, InteractUsingEvent>(OnInteractUsing);

        SubscribeLocalEvent<MedicalScannerComponent, EntInsertedIntoContainerMessage>(OnMedicalScannerInsert);
        SubscribeLocalEvent<MedicalScannerComponent, EntRemovedFromContainerMessage>(OnMedicalScannerRemove);

        // UI actions
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerSequencerButtonPressedMessage>(OnSequencerButton);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerSaveMutationToStorageMessage>(OnSaveMutation);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerDeleteMutationFromStorageMessage>(OnDeleteMutation);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerPrintActivatorMessage>(OnPrintActivator);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerPrintMutatorMessage>(OnPrintMutator);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerScrambleDnaMessage>(OnScrambleDna);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerToggleResearchMessage>(OnToggleResearch);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerUseJokerMessage>(OnJokerUsed);
    }

    private void OnComponentInit(EntityUid uid, DnaScannerConsoleComponent comp, ComponentInit args)
    {
        EnsureDiscoveryTracker(uid);
    }

    private void OnBeforeUIOpen(EntityUid uid, DnaScannerConsoleComponent comp, BeforeActivatableUIOpenEvent args)
    {
        SendUiUpdate(uid, comp, true);
    }

    private void OnInteractUsing(EntityUid uid, DnaScannerConsoleComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<DnaSequenceInjectorComponent>(args.Used, out var injector))
            return;

        args.Handled = true;
        string? popupMessage = null;

        if (string.IsNullOrEmpty(injector.MutationId))
        {
            popupMessage = Loc.GetString("dna-scanner-empty-injector");
        }
        else
        {
            var mutationId = injector.MutationId;

            if (comp.SavedMutations.Any(m => m.Id == mutationId))
            {
                popupMessage = Loc.GetString("dna-scanner-mutation-already-stored");
            }
            else if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto))
            {
                popupMessage = Loc.GetString("dna-scanner-print-corrupted", ("mutation", "Unknown"));
            }
            else
            {
                var slot = _shuffle.GetOrAssignSlot(mutationId);

                if (slot.Block <= 0)
                {
                    popupMessage = Loc.GetString("dna-scanner-invalid-mutation-slot");
                }
                else
                {
                    var entry = new MutationEntry(
                        Block: slot.Block,
                        Id: mutationId,
                        Name: proto.Name,
                        OriginalSequence: slot.Sequence,
                        RevealedSequence: slot.Sequence,
                        Enabled: false,
                        Description: proto.Description,
                        Instability: proto.Instability,
                        Conflicts: proto.Conflicts
                    );

                    TrySaveMutation(uid, comp, entry);
                    _discovery.DiscoverMutation(uid, mutationId);
                    popupMessage = Loc.GetString("dna-scanner-mutation-saved");
                }
            }
        }

        if (popupMessage is not null)
            _popup.PopupEntity(popupMessage, uid, args.User);

        Del(args.Used);
        TryAddInjector(uid, comp);
        SendUiUpdate(uid, comp, true);
    }

    private void OnToggleResearch(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerToggleResearchMessage msg)
    {
        var mutationId = msg.MutationId;

        if (comp.CurrentSubject is { Valid: true } subject &&
            TryComp<GeneticsComponent>(subject, out var genetics))
        {
            var mutation = genetics.Mutations.Find(m => m.Id == mutationId);
            if (mutation != null && mutation.RevealedSequence == mutation.OriginalSequence)
            {
                TrySaveMutation(uid, comp, mutation);
            }
        }

        if (comp.ActiveResearchQueue.Contains(mutationId))
        {
            TryCancelResearchingMutation(uid, mutationId, comp);
        }
        else
        {
            TryStartResearchingMutation(uid, mutationId, comp);
        }

        SendUiUpdate(uid, comp, true);
    }

    private void OnMedicalScannerInsert(EntityUid uid, MedicalScannerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != $"scanner-bodyContainer")
            return;

        NotifyLinkedConsoles(uid, args.Entity);
    }

    private void OnMedicalScannerRemove(EntityUid uid, MedicalScannerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != $"scanner-bodyContainer")
            return;

        NotifyLinkedConsoles(uid, null);
    }

    private void NotifyLinkedConsoles(EntityUid scannerUid, EntityUid? occupant)
    {
        if (!TryComp<DeviceLinkSinkComponent>(scannerUid, out var sink))
            return;

        foreach (var consoleUid in sink.LinkedSources)
        {
            if (!TryComp<DnaScannerConsoleComponent>(consoleUid, out var console))
                continue;

            console.CurrentSubject = occupant;

            if (occupant is { Valid: true })
            {
                DiscoverCompletedMutations(consoleUid, console);
                SendUiUpdate(consoleUid, console, fullUpdate: true);
            }
            else
            {
                SendUiUpdate(consoleUid, console, fullUpdate: false);
            }
        }
    }

    private void OnSaveMutation(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerSaveMutationToStorageMessage msg)
    {
        if (comp.CurrentSubject is not { Valid: true } subject ||
            !TryComp<GeneticsComponent>(subject, out var genetics))
            return;

        var mutation = genetics.Mutations.Find(m => m.Id == msg.MutationId);
        if (mutation is null || mutation.RevealedSequence != mutation.OriginalSequence)
            return;

        if (TrySaveMutation(uid, comp, mutation))
            SendUiUpdate(uid, comp, true);
    }

    private void OnDeleteMutation(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerDeleteMutationFromStorageMessage msg)
    {
        var mutationId = msg.MutationId;

        if (comp.ActiveResearchQueue.Contains(mutationId))
        {
            TryCancelResearchingMutation(uid, mutationId, comp);
        }

        if (DeleteSavedMutation(uid, comp, msg.MutationId))
            SendUiUpdate(uid, comp, true);
    }

    private void OnSequencerButton(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerSequencerButtonPressedMessage msg)
    {
        if (comp.CurrentSubject is not { Valid: true } subject ||
            !TryComp<GeneticsComponent>(subject, out var genetics) ||
            !TryComp<MobStateComponent>(subject, out var mobState) ||
            mobState.CurrentState == MobState.Dead ||
            !_proto.TryIndex<GeneticMutationPrototype>(msg.MutationId, out var proto) ||
            proto.SequencerResistant ||
            !_genetics.TryModifyMutationSequence(subject, genetics, msg.MutationId, msg.ButtonIndex, msg.NewBase))
        {
            _audio.PlayPvs(comp.SoundDeny, uid);
            return;
        }

        var mutation = genetics.Mutations.Find(m => m.Id == msg.MutationId);
        if (mutation is null)
            return;

        var isCorrect = mutation.RevealedSequence == mutation.OriginalSequence;

        if (isCorrect && !mutation.Enabled)
            _genetics.TryActivateMutation(subject, genetics, msg.MutationId);
        else if (!isCorrect && mutation.Enabled)
            _genetics.TryDeactivateMutation(subject, genetics, msg.MutationId);

        // Apply radiation damage
        var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Radiation"), SequencerButtonRadiationDamage);
        if (TryComp<DamageableComponent>(subject, out var damageable))
            _damageable.TryChangeDamage(subject, damage, ignoreResistances: true, damageable: damageable);

        if (isCorrect)
            _discovery.DiscoverMutation(uid, msg.MutationId);

        SendUiUpdate(uid, comp, true);
    }

    private void OnPrintActivator(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerPrintActivatorMessage msg)
    {
        TryPrintInjector(uid, comp, msg.MutationId, activator: true);
    }

    private void OnPrintMutator(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerPrintMutatorMessage msg)
    {
        TryPrintInjector(uid, comp, msg.MutationId, activator: false);
    }

    private void TryPrintInjector(EntityUid uid, DnaScannerConsoleComponent comp, string mutationId, bool activator)
    {
        if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto) || !proto.Printable)
        {
            _popup.PopupEntity(Loc.GetString("dna-scanner-print-corrupted", ("mutation", proto?.Name ?? "Unknown")), uid);
            _audio.PlayPvs(comp.SoundDeny, uid);
            return;
        }

        if (!TryConsumeInjector(uid, comp))
        {
            _popup.PopupEntity(Loc.GetString("dna-scanner-no-injectors"), uid);
            _audio.PlayPvs(comp.SoundDeny, uid);
            return;
        }

        if (!TryComp<TransformComponent>(uid, out var xform))
            return;

        var protoId = activator ? "DNAInjectorGenericActivator" : "DNAInjectorGenericMutator";
        var injector = Spawn(protoId, xform.Coordinates);

        if (TryComp<DnaSequenceInjectorComponent>(injector, out var injComp))
        {
            injComp.MutationId = mutationId;
            Dirty(injector, injComp);
        }

        // TODO: Add sound for injector printing
    }

    private void OnScrambleDna(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerScrambleDnaMessage msg)
    {
        if (comp.CurrentSubject is not { Valid: true } subject || !TryComp<GeneticsComponent>(subject, out var genetics))
            return;

        _genetics.ScrambleDna(subject, genetics);

        // Massive cellular damage - or is it supposed to be radiation? TODO: Find out
        if (TryComp<DamageableComponent>(subject, out var damageable))
        {
            var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Radiation"), ScrambleRadiationDamage);
            _damageable.TryChangeDamage(subject, damage, ignoreResistances: true, damageable: damageable);
        }

        comp.ScrambleCooldownEnd = _timing.CurTime + TimeSpan.FromSeconds(ScrambleCooldownSeconds);
        _popup.PopupEntity(Loc.GetString("dna-scanner-scramble-complete"), subject, PopupType.Large);
        _audio.PlayPvs(comp.SoundDnaScramble, uid);
        SendUiUpdate(uid, comp, true);
    }

    private void OnJokerUsed(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerUseJokerMessage msg)
    {
        comp.JokerCooldownEnd = _timing.CurTime + TimeSpan.FromSeconds(JokerCooldownSeconds);
        Dirty(uid, comp);

        SendUiUpdate(uid, comp, false);
    }

    private void EnsureDiscoveryTracker(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid is not { } grid)
            return;

        if (!HasComp<DnaScannerDiscoveryTrackerComponent>(grid))
            AddComp<DnaScannerDiscoveryTrackerComponent>(grid);
    }

    private bool TrySaveMutation(EntityUid uid, DnaScannerConsoleComponent comp, MutationEntry entry)
    {
        if (comp.SavedMutations.Any(m => m.Id == entry.Id))
            return true;

        comp.SavedMutations.Add(entry);
        Dirty(uid, comp);
        _discovery.DiscoverMutation(uid, entry.Id);
        _unlockTrigger.OnMutationSaved(uid, comp, entry.Id);

        if (_proto.TryIndex<GeneticMutationPrototype>(entry.Id, out var proto) && proto.ResearchPoints > 0)
        {
            var gridProgress = _discovery.GetMutableGridResearchProgress(uid);
            if (!gridProgress.ContainsKey(entry.Id))
            {
                gridProgress[entry.Id] = proto.ResearchPoints;
            }
        }

        return true;
    }

    private bool DeleteSavedMutation(EntityUid uid, DnaScannerConsoleComponent comp, string mutationId)
    {
        var removed = comp.SavedMutations.RemoveAll(m => m.Id == mutationId) > 0;
        if (removed)
            Dirty(uid, comp);
        return removed;
    }

    private bool TryAddInjector(EntityUid uid, DnaScannerConsoleComponent comp)
    {
        comp.DnaInjectors++;
        Dirty(uid, comp);
        return true;
    }

    private bool TryConsumeInjector(EntityUid uid, DnaScannerConsoleComponent comp)
    {
        if (comp.DnaInjectors <= 0)
            return false;

        comp.DnaInjectors--;
        Dirty(uid, comp);
        return true;
    }

    private void SendUiUpdate(EntityUid uid, DnaScannerConsoleComponent? comp = null, bool fullUpdate = false)
    {
        if (!Resolve(uid, ref comp))
            return;

        var state = BuildUiState(uid, comp, fullUpdate);
        _ui.SetUiState(uid, DnaScannerConsoleUiKey.Key, state);
    }

    private void DiscoverCompletedMutations(EntityUid uid, DnaScannerConsoleComponent comp)
    {
        if (comp.CurrentSubject is not { Valid: true } subject ||
            !TryComp<GeneticsComponent>(subject, out var genetics))
            return;

        foreach (var mutation in genetics.Mutations)
        {
            if (mutation.RevealedSequence == mutation.OriginalSequence)
                _discovery.DiscoverMutation(uid, mutation.Id);
        }
    }

    private GeneticistsConsoleBoundUserInterfaceState BuildUiState(EntityUid uid, DnaScannerConsoleComponent comp, bool fullUpdate)
    {
        string? subjectName = null;
        string? healthStatus = null;
        float? radiationDamage = null;
        int instability = 0;
        List<MutationEntry>? mutations = null;
        var discovered = new HashSet<string>();
        var baseIds = new HashSet<string>();

        if (comp.CurrentSubject is { Valid: true } subject)
        {
            subjectName = Name(subject);
            healthStatus = GetHealthString(subject);
            radiationDamage = GetRadiationDamage(subject);

            if (TryComp<GeneticsComponent>(subject, out var genetics))
            {
                instability = genetics.GeneticInstability;
                baseIds = new HashSet<string>(genetics.BaseMutationIds);
                mutations = genetics.Mutations;
            }
        }

        var researchRemaining = new Dictionary<string, int>();
        var researchOriginal = new Dictionary<string, int>();
        var activeResearchIds = new HashSet<string>(comp.ActiveResearchQueue);
        var gridProgress = _discovery.GetGridResearchProgress(uid);

        foreach (var (mutationId, remaining) in gridProgress)
        {
            if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto))
                continue;

            var original = proto.ResearchPoints;
            if (original <= 0)
                continue;

            researchOriginal[mutationId] = original;
            researchRemaining[mutationId] = Math.Max(0, remaining);
        }

        if (fullUpdate)
        {
            discovered = _discovery.GetGridDiscovered(uid);
        }

        return new GeneticistsConsoleBoundUserInterfaceState(
            subjectName: subjectName,
            healthStatus: healthStatus,
            radiationDamage: radiationDamage,
            subjectGeneticInstability: instability,
            scrambleCooldownEnd: comp.ScrambleCooldownEnd,
            mutations: fullUpdate ? mutations : null,
            discoveredMutationIds: fullUpdate ? discovered : null,
            baseMutationIds: fullUpdate ? baseIds : null,
            savedMutations: fullUpdate ? comp.SavedMutations : null,
            isFullUpdate: fullUpdate,
            researchRemaining: researchRemaining,
            researchOriginal: researchOriginal,
            activeResearchMutationIds: activeResearchIds,
            jokerCooldownEnd: comp.JokerCooldownEnd
        );
    }

    private string? GetHealthString(EntityUid uid)
    {
        if (!TryComp<MobStateComponent>(uid, out var mobState))
            return null;

        return mobState.CurrentState switch
        {
            MobState.Dead => "Dead",
            MobState.Critical or MobState.Alive => TryComp<DamageableComponent>(uid, out var d)
                ? MathF.Round((float)d.TotalDamage, 1).ToString()
                : "0",
            _ => null
        };
    }

    private float? GetRadiationDamage(EntityUid uid)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return null;

        var rad = damageable.Damage["Radiation"];
        return MathF.Round(rad.Float(), 2);
    }

    private void ProcessResearchTick(EntityUid uid, DnaScannerConsoleComponent console, ResearchPointSourceComponent source)
    {
        if (console.ActiveResearchQueue.Count == 0)
        {
            if (source.PointsPerSecond != 0)
                source.PointsPerSecond = 0;

            return;
        }

        UpdateResearchRate(uid, console, source);

        var completed = new List<string>();

        foreach (var mutationId in console.ActiveResearchQueue.ToList())
        {
            if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto) || proto.ResearchPoints <= 0)
            {
                console.ActiveResearchQueue.Remove(mutationId);
                completed.Add(mutationId);
                continue;
            }

            int original = proto.ResearchPoints;
            int pps = original / ResearchDurationSeconds;
            if (pps < 1 && original > 0)
                pps = 1;

            int deduct = pps;

            _discovery.DeductResearchProgress(uid, mutationId, deduct);

            var gridProgress = _discovery.GetGridResearchProgress(uid);
            if (gridProgress.TryGetValue(mutationId, out var rem) && rem <= 0)
            {
                console.ActiveResearchQueue.Remove(mutationId);
                completed.Add(mutationId);
            }
        }

        Dirty(uid, console);

        if (completed.Count > 0)
        {
            foreach (var id in completed)
            {
                if (_proto.TryIndex<GeneticMutationPrototype>(id, out var proto))
                {
                    var name = !string.IsNullOrEmpty(proto.Name) ? proto.Name : id;
                    _popup.PopupEntity(Loc.GetString("dna-scanner-research-complete", ("mutation", name)), uid);
                }
            }

            SendUiUpdate(uid, console, true);
        }
    }

    private void UpdateResearchRate(EntityUid uid, DnaScannerConsoleComponent console, ResearchPointSourceComponent? source = null)
    {
        if (!Resolve(uid, ref source, false))
            return;

        int totalPps = 0;

        foreach (var mutationId in console.ActiveResearchQueue)
        {
            if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto) || proto.ResearchPoints <= 0)
                continue;

            int pps = proto.ResearchPoints / ResearchDurationSeconds;
            if (pps < 1 && proto.ResearchPoints > 0)
                pps = 1;

            totalPps += pps;
        }

        if (source.PointsPerSecond != totalPps)
            source.PointsPerSecond = totalPps;
    }

    public bool TryStartResearchingMutation(EntityUid uid, string mutationId, DnaScannerConsoleComponent? console = null)
    {
        if (!Resolve(uid, ref console))
            return false;

        if (console.ActiveResearchQueue.Contains(mutationId))
            return false;

        if (console.ActiveResearchQueue.Count >= MaxActiveResearchSlots)
            return false;

        if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto) || proto.ResearchPoints <= 0)
            return false;

        console.ActiveResearchQueue.Add(mutationId);

        var gridProgress = _discovery.GetMutableGridResearchProgress(uid);
        if (!gridProgress.ContainsKey(mutationId))
            gridProgress[mutationId] = proto.ResearchPoints;

        Dirty(uid, console);

        if (TryComp<ResearchPointSourceComponent>(uid, out var source))
            UpdateResearchRate(uid, console, source);

        return true;
    }

    public bool TryCancelResearchingMutation(EntityUid uid, string mutationId, DnaScannerConsoleComponent? console = null)
    {
        if (!Resolve(uid, ref console))
            return false;

        if (!console.ActiveResearchQueue.Remove(mutationId))
            return false;

        Dirty(uid, console);

        if (TryComp<ResearchPointSourceComponent>(uid, out var source))
            UpdateResearchRate(uid, console, source);

        return true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<DnaScannerConsoleComponent, ResearchPointSourceComponent>();
        while (query.MoveNext(out var uid, out var console, out var source))
        {
            // Health UI update
            if (console.NextHealthUpdate is null || console.NextHealthUpdate <= now)
            {
                console.NextHealthUpdate = now + UpdateTickInterval;
                SendUiUpdate(uid, console, false);
            }

            // Research tick every second
            if (console.LastResearchTick is not { } lastTick || now >= lastTick + UpdateTickInterval)
            {
                console.LastResearchTick = now;
                Dirty(uid, console);
                ProcessResearchTick(uid, console, source);
            }
        }
    }
}
