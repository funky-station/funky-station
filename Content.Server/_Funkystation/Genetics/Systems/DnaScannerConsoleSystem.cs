using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.Popups;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Content.Shared._Funkystation.Genetics.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Robust.Shared.Audio.Systems;
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

    private const float SequencerButtonCellularDamage = 0.2f;
    private const float ScrambleCellularDamage = 25f;
    private const float ScrambleCooldownSeconds = 30f;
    private static readonly TimeSpan HealthTickInterval = TimeSpan.FromSeconds(2);

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaScannerConsoleComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DnaScannerConsoleComponent, BeforeActivatableUIOpenEvent>(OnBeforeUIOpen);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerSubjectChangedEvent>(OnSubjectChanged);
        SubscribeLocalEvent<DnaScannerConsoleComponent, InteractUsingEvent>(OnInteractUsing);

        // UI actions
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerSequencerButtonPressedMessage>(OnSequencerButton);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerSaveMutationToStorageMessage>(OnSaveMutation);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerDeleteMutationFromStorageMessage>(OnDeleteMutation);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerPrintActivatorMessage>(OnPrintActivator);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerPrintMutatorMessage>(OnPrintMutator);
        SubscribeLocalEvent<DnaScannerConsoleComponent, DnaScannerScrambleDnaMessage>(OnScrambleDna);
    }

    private void OnComponentInit(EntityUid uid, DnaScannerConsoleComponent comp, ComponentInit args)
    {
        comp.NextHealthUpdate = _timing.CurTime + HealthTickInterval;
        EnsureDiscoveryTracker(uid);
    }

    private void OnBeforeUIOpen(EntityUid uid, DnaScannerConsoleComponent comp, BeforeActivatableUIOpenEvent args)
    {
        DiscoverCompletedMutations(uid, comp);
        SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private void OnSubjectChanged(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerSubjectChangedEvent args)
    {
        DiscoverCompletedMutations(uid, comp);
        SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private void OnInteractUsing(EntityUid uid, DnaScannerConsoleComponent comp, InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<DnaSequenceInjectorComponent>(args.Used, out var injector))
            return;

        args.Handled = true;

        if (string.IsNullOrEmpty(injector.MutationId))
        {
            _popup.PopupEntity(Loc.GetString("dna-scanner-empty-injector"), uid, args.User);
            Del(args.Used);
            TryAddInjector(uid, comp);
            return;
        }

        var mutationId = injector.MutationId;
        if (comp.SavedMutations.Any(m => m.Id == mutationId))
        {
            _popup.PopupEntity(Loc.GetString("dna-scanner-mutation-already-stored"), uid, args.User);
            return;
        }

        if (!_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto))
            return;

        var entry = CreateMutationEntry(mutationId, proto);

        SaveMutation(uid, comp, entry);
        _discovery.DiscoverMutation(uid, mutationId);

        _popup.PopupEntity(Loc.GetString("dna-scanner-mutation-saved"), uid, args.User);
        Del(args.Used);
        TryAddInjector(uid, comp);

        SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private MutationEntry CreateMutationEntry(string mutationId, GeneticMutationPrototype proto)
    {
        var block = _shuffle.GetBlock(mutationId);
        var sequence = _shuffle.GetSequence(mutationId);

        return new MutationEntry(
            Block: block,
            Id: mutationId,
            Name: proto.Name,
            OriginalSequence: sequence,
            RevealedSequence: sequence,
            Enabled: false,
            Description: proto.Description,
            Instability: proto.Instability,
            Conflicts: proto.Conflicts
        );
    }

    private void OnSaveMutation(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerSaveMutationToStorageMessage msg)
    {
        if (comp.CurrentSubject is not { Valid: true } subject ||
            !TryComp<GeneticsComponent>(subject, out var genetics))
            return;

        var mutation = genetics.Mutations.Find(m => m.Id == msg.MutationId);
        if (mutation is null || mutation.RevealedSequence != mutation.OriginalSequence)
            return;

        if (SaveMutation(uid, comp, mutation))
            SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private void OnDeleteMutation(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerDeleteMutationFromStorageMessage msg)
    {
        if (DeleteSavedMutation(uid, comp, msg.MutationId))
            SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private void OnSequencerButton(EntityUid uid, DnaScannerConsoleComponent comp, DnaScannerSequencerButtonPressedMessage msg)
    {
        if (comp.CurrentSubject is not { Valid: true } subject ||
            !TryComp<GeneticsComponent>(subject, out var genetics) ||
            !TryComp<MobStateComponent>(subject, out var mobState) ||
            mobState.CurrentState == MobState.Dead)
        {
            _audio.PlayPvs(comp.SoundDeny, uid);
            return;
        }

        _genetics.TryModifyMutationSequence(subject, genetics, msg.MutationId, msg.ButtonIndex, msg.NewBase);

        var mutation = genetics.Mutations.Find(m => m.Id == msg.MutationId);
        if (mutation is null)
            return;

        var isCorrect = mutation.RevealedSequence == mutation.OriginalSequence;

        if (isCorrect && !mutation.Enabled)
            _genetics.TryActivateMutation(subject, genetics, msg.MutationId);
        else if (!isCorrect && mutation.Enabled)
            _genetics.TryDeactivateMutation(subject, genetics, msg.MutationId);

        // Apply cellular damage
        var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Cellular"), SequencerButtonCellularDamage);
        if (TryComp<DamageableComponent>(subject, out var damageable))
            _damageable.TryChangeDamage(subject, damage, ignoreResistances: true, damageable: damageable);

        if (isCorrect)
            _discovery.DiscoverMutation(uid, msg.MutationId);

        SendUiUpdate(uid, comp, fullUpdate: true);
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
            var damage = new DamageSpecifier(_proto.Index<DamageTypePrototype>("Cellular"), ScrambleCellularDamage);
            _damageable.TryChangeDamage(subject, damage, ignoreResistances: true, damageable: damageable);
        }

        comp.ScrambleCooldownEnd = _timing.CurTime + TimeSpan.FromSeconds(ScrambleCooldownSeconds);
        _popup.PopupEntity(Loc.GetString("dna-scanner-scramble-complete"), subject, PopupType.Large);
        _audio.PlayPvs(comp.SoundDnaScramble, uid);
        SendUiUpdate(uid, comp, fullUpdate: true);
    }

    private void EnsureDiscoveryTracker(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || xform.GridUid is not { } grid)
            return;

        if (!HasComp<DnaScannerDiscoveryTrackerComponent>(grid))
            AddComp<DnaScannerDiscoveryTrackerComponent>(grid);
    }

    private bool SaveMutation(EntityUid uid, DnaScannerConsoleComponent comp, MutationEntry entry)
    {
        if (comp.SavedMutations.Any(m => m.Id == entry.Id))
            return true;

        comp.SavedMutations.Add(entry);
        Dirty(uid, comp);
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

    private DnaScannerConsoleBoundUserInterfaceState BuildUiState(EntityUid uid, DnaScannerConsoleComponent comp, bool fullUpdate)
    {
        string? subjectName = null;
        string? healthStatus = null;
        float? geneticDamage = null;
        int instability = 0;
        List<MutationEntry>? mutations = null;
        var discovered = new HashSet<string>();
        var baseIds = new HashSet<string>();

        if (comp.CurrentSubject is { Valid: true } subject)
        {
            subjectName = Name(subject);
            healthStatus = GetHealthString(subject);
            geneticDamage = GetGeneticDamage(subject);

            if (TryComp<GeneticsComponent>(subject, out var genetics))
            {
                instability = genetics.GeneticInstability;
                baseIds = new HashSet<string>(genetics.BaseMutationIds);
                mutations = genetics.Mutations;
            }
        }

        if (fullUpdate)
            discovered = _discovery.GetGridDiscovered(uid);

        return new DnaScannerConsoleBoundUserInterfaceState(
            subjectName: subjectName,
            healthStatus: healthStatus,
            geneticDamage: geneticDamage,
            subjectGeneticInstability: instability,
            scrambleCooldownEnd: comp.ScrambleCooldownEnd,
            mutations: fullUpdate ? mutations : null,
            discoveredMutationIds: fullUpdate ? discovered : null,
            baseMutationIds: fullUpdate ? baseIds : null,
            savedMutations: fullUpdate ? comp.SavedMutations : null,
            isFullUpdate: fullUpdate);
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

    private float? GetGeneticDamage(EntityUid uid)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable) || damageable.Damage is not { } damage)
            return null;

        var groups = damage.GetDamagePerGroup(_proto);
        return groups.TryGetValue("Genetic", out var val)
            ? MathF.Round(val.Float(), 2)
            : null;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        foreach (var comp in EntityQuery<DnaScannerConsoleComponent>(true))
        {
            if (now < comp.NextHealthUpdate)
                continue;

            comp.NextHealthUpdate = now + HealthTickInterval;
            SendUiUpdate(comp.Owner, comp, fullUpdate: false);
        }
    }
}
