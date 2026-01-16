using System.Linq;
using Content.Server._Funkystation.Genetics.Components;
using Content.Server.Popups;
using Content.Shared._Funkystation.Genetics;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Content.Shared.Mobs;
using Content.Server._Funkystation.Genetics.Mutations.Systems;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed partial class GeneticsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GeneticShuffleSystem _shuffle = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float MinSequenceRevealFraction = 0.45f;
    private const float MaxSequenceRevealFraction = 0.80f;
    private const float MinRadsUntilMutation = 20f;
    private const float MaxRadsUntilMutation = 95f;
    private const int InstabilityMutationThreshold = 100;
    private const int InstabilityDamageThreshold = 150;
    private const int MinInstabilityTimerSeconds = 90;
    private const int MaxInstabilityTimerSeconds = 160;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<GeneticsComponent, DamageChangedEvent>(OnRadiationDamage);
    }

    private void OnInit(EntityUid uid, GeneticsComponent component, ComponentInit args)
    {
        FillBaseMutations(uid, component);
        component.RadsUntilRandomMutation = _random.NextFloat(MinRadsUntilMutation, MaxRadsUntilMutation);
    }

    private void OnRadiationDamage(EntityUid uid, GeneticsComponent component, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta is not { } delta)
            return;

        if (!delta.DamageDict.TryGetValue("Radiation", out var radDamage) || radDamage <= FixedPoint2.Zero)
            return;

        if (TryComp<MobStateComponent>(uid, out var mobState) && mobState.CurrentState == MobState.Dead)
            return;

        component.RadsUntilRandomMutation -= radDamage.Float();

        if (component.RadsUntilRandomMutation > 0)
            return;

        TriggerRandomMutation(uid, component);
        component.RadsUntilRandomMutation = _random.NextFloat(MinRadsUntilMutation, MaxRadsUntilMutation);
    }

    public void FillBaseMutations(EntityUid uid, GeneticsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Mutations.Clear();
        component.BaseMutationIds.Clear();

        var mutationsToAdd = new List<MutationEntry>();
        var addedForcedCount = 0;

        // Forced base mutations
        foreach (var forced in component.ForcedBaseMutations)
        {
            if (!(_random.NextFloat() < forced.Chance))
                continue;

            if (!_proto.TryIndex<GeneticMutationPrototype>(forced.Id, out var proto) ||
                !CanEntityReceiveMutation(uid, proto))
                continue;

            var slot = _shuffle.GetOrAssignSlot(forced.Id);
            if (slot.Block <= 0)
                continue;

            // Second roll: should it start active?
            bool startsActive = _random.Prob(forced.StartActive);

            var revealed = startsActive ? slot.Sequence : RandomizeSequence(slot.Sequence);
            var entry = CreateMutationEntry(
                mutationId: forced.Id,
                proto: proto,
                block: slot.Block,
                originalSequence: slot.Sequence,
                revealedSequence: revealed,
                enabled: startsActive
            );

            mutationsToAdd.Add(entry);
            component.BaseMutationIds.Add(forced.Id);

            // Apply components if it starts active. Skips all checks.
            if (startsActive)
                ApplyMutationComponents(uid, component, proto);

            addedForcedCount++;
        }

        var slotsLeft = Math.Max(0, component.MutationSlots - addedForcedCount);

        // Random filler mutations
        for (int i = 0; i < slotsLeft; i++)
        {
            var chosenId = PickRandomAvailableMutation(uid, component);
            if (chosenId == null)
                break;

            if (!_proto.TryIndex<GeneticMutationPrototype>(chosenId, out var proto))
                continue;

            var slot = _shuffle.GetOrAssignSlot(chosenId);
            if (slot.Block <= 0)
                continue;

            var revealed = RandomizeSequence(slot.Sequence);
            var entry = CreateMutationEntry(
                mutationId: chosenId,
                proto: proto,
                block: slot.Block,
                originalSequence: slot.Sequence,
                revealedSequence: revealed,
                enabled: false
            );

            mutationsToAdd.Add(entry);
            component.BaseMutationIds.Add(chosenId);
        }

        // Shuffle the entire list of mutations before adding them to the component
        // this stops forced mutations from clustering at the beginning of the list
        _random.Shuffle(mutationsToAdd);

        // Add them in the shuffled order
        foreach (var entry in mutationsToAdd)
            component.Mutations.Add(entry);
    }

    private string? PickRandomAvailableMutation(EntityUid uid, GeneticsComponent component)
    {
        var candidates = _proto.EnumeratePrototypes<GeneticMutationPrototype>()
            .Where(p => CanEntityReceiveMutation(uid, p, true))
            .Where(p => !component.Mutations.Any(m => m.Id == p.ID))
            .Where(p => !IsConflictingWithExisting(component, p))
            .Where(p => !component.BaseMutationIds.Contains(p.ID))
            .ToList();

        if (candidates.Count == 0)
            return null;

        // If only one, no need for weighting
        if (candidates.Count == 1)
            return candidates[0].ID;

        // Calculate total weight
        float totalWeight = 0f;
        foreach (var proto in candidates)
        {
            totalWeight += proto.ProbabilityWeight;
        }

        // Roll a random value between 0 and totalWeight
        float roll = _random.NextFloat(0f, totalWeight);

        // Find which prototype the roll lands on
        float current = 0f;
        foreach (var proto in candidates)
        {
            current += proto.ProbabilityWeight;
            if (roll <= current)
                return proto.ID;
        }

        // Fallback (should never hit, but kept for safety)
        return candidates.Last().ID;
    }

    public void TriggerRandomMutation(EntityUid uid, GeneticsComponent component)
    {
        var chosenId = PickRandomAvailableMutation(uid, component);
        if (chosenId == null)
            return;

        var slot = _shuffle.GetOrAssignSlot(chosenId);
        if (slot.Block <= 0)
            return;

        TryAddMutation(uid, component, chosenId);
        TryActivateMutation(uid, component, chosenId);
    }

    public void RemoveRandomMutation(EntityUid uid, GeneticsComponent component, bool mutadone = false)
    {
        var removable = new List<string>();

        foreach (var entry in component.Mutations)
        {
            // Skip mutations that are resistant to mutadone when called from mutadone
            if (mutadone)
            {
                var proto = _proto.Index<GeneticMutationPrototype>(entry.Id);
                if (proto.MutadoneResistant)
                    continue;
            }
            // All non-base mutations (active or not) are eligible
            if (!component.BaseMutationIds.Contains(entry.Id))
            {
                removable.Add(entry.Id);
                continue;
            }

            // Base mutations only if they are currently enabled
            if (entry.Enabled)
            {
                removable.Add(entry.Id);
            }
        }

        if (removable.Count == 0)
            return;

        var chosenId = _random.Pick(removable);

        TryRemoveMutation(uid, component, chosenId);
    }

    private string RandomizeSequence(string original)
    {
        if (string.IsNullOrEmpty(original) || original.Length <= 2)
            return original;

        var length = original.Length;
        var chars = original.ToCharArray();

        var revealed = new bool[length];
        revealed[0] = true;
        revealed[length - 1] = true;

        var revealCount = _random.Next((int) (length * MinSequenceRevealFraction), (int) (length * MaxSequenceRevealFraction) + 1);
        var added = 2;

        while (added < revealCount)
        {
            var idx = _random.Next(1, length - 1);
            if (!revealed[idx])
            {
                revealed[idx] = true;
                added++;
            }
        }

        for (var i = 0; i < length; i++)
        {
            if (!revealed[i])
                chars[i] = 'X';
        }

        return new string(chars);
    }

    public bool TryAddMutation(EntityUid uid, GeneticsComponent component, string mutationId)
    {
        var slot = _shuffle.GetOrAssignSlot(mutationId);

        if (slot == GeneticBlock.Invalid ||
            component.Mutations.Any(m => m.Id == mutationId) ||
            !_proto.TryIndex(mutationId, out GeneticMutationPrototype? proto))
            return false;

        if (!CanEntityReceiveMutation(uid, proto, false))
            return false;

        if (IsConflictingWithExisting(component, proto))
            return false;

        var revealed = RandomizeSequence(slot.Sequence);
        var entry = CreateMutationEntry(
            mutationId: mutationId,
            proto: proto,
            block: slot.Block,
            originalSequence: slot.Sequence,
            revealedSequence: revealed,
            enabled: false
        );

        component.Mutations.Add(entry);

        if (!component.BaseMutationIds.Contains(mutationId))
            ModifyInstability(uid, component, proto.Instability);

        return true;
    }

    public bool TryRemoveMutation(EntityUid uid, GeneticsComponent component, string mutationId)
    {
        var entry = component.Mutations.Find(m => m.Id == mutationId);
        if (entry == null)
            return false;

        bool isBase = component.BaseMutationIds.Contains(mutationId);

        // Deactivate mutation regardless of removal
        if (entry.Enabled)
        {
            if (!TryDeactivateMutation(uid, component, mutationId))
                return false;
        }

        // Return true without removing if base mutation
        if (isBase)
        {
            return true;
        }

        if (_proto.TryIndex<GeneticMutationPrototype>(mutationId, out var proto))
            ModifyInstability(uid, component, -proto.Instability);

        component.Mutations.Remove(entry);

        return true;
    }

    public bool TryActivateMutation(EntityUid uid, GeneticsComponent component, string mutationId)
    {
        var entry = component.Mutations.Find(m => m.Id == mutationId);
        if (entry == null || entry.Enabled)
            return false;

        if (!_proto.TryIndex(mutationId, out GeneticMutationPrototype? proto))
            return false;

        // One more just in case
        if (!CanEntityReceiveMutation(uid, proto, false))
            return false;

        foreach (var conflictId in proto.Conflicts)
        {
            if (component.Mutations.Any(m => m.Id == conflictId && m.Enabled))
                return false;
        }

        var index = component.Mutations.FindIndex(m => m.Id == mutationId);
        component.Mutations[index] = component.Mutations[index] with
        {
            Enabled = true,
            RevealedSequence = component.Mutations[index].OriginalSequence
        };

        ApplyMutationComponents(uid, component, proto);

        var popMsg = !string.IsNullOrWhiteSpace(proto.PopupText)
            ? Loc.GetString(proto.PopupText)
            : Loc.GetString("genetics-mutation-activated");

        _popup.PopupEntity(popMsg, uid, uid);

        return true;
    }

    public bool TryDeactivateMutation(EntityUid uid, GeneticsComponent component, string mutationId)
    {
        var entry = component.Mutations.Find(m => m.Id == mutationId);
        if (entry == null || !entry.Enabled)
            return false;

        if (!_proto.TryIndex(mutationId, out GeneticMutationPrototype? proto))
            return false;

        var index = component.Mutations.FindIndex(m => m.Id == mutationId);
        component.Mutations[index] = component.Mutations[index] with
        {
            Enabled = false
        };

        RemoveMutationComponents(uid, proto);
        return true;
    }

    private void ApplyMutationComponents(EntityUid uid, GeneticsComponent component, GeneticMutationPrototype proto)
    {
        EntityManager.AddComponents(uid, proto.Components);
    }

    private void RemoveMutationComponents(EntityUid uid, GeneticMutationPrototype proto)
    {
        foreach (var (comp, _) in proto.Components.Values)
        {
            if (HasComp(uid, comp.GetType()))
                RemComp(uid, comp.GetType());
        }
    }

    private bool IsConflictingWithExisting(GeneticsComponent component, GeneticMutationPrototype proto)
    {
        return proto.Conflicts.Any(conflictId =>
            component.Mutations.Any(m => m.Id == conflictId));
    }

    private void ModifyInstability(EntityUid uid, GeneticsComponent component, int delta)
    {
        if (delta == 0) return;

        var old = component.GeneticInstability;
        component.GeneticInstability += delta;
        var newVal = component.GeneticInstability;

        // Bad things happen past InstabilityMutationThreshold
        if (old <= InstabilityMutationThreshold && newVal > InstabilityMutationThreshold)
        {
            // Remove any existing pending timer
            RemComp<PendingInstabilityMutationComponent>(uid);

            var pending = AddComp<PendingInstabilityMutationComponent>(uid);
            pending.MutationId = string.Empty;

            // Set a random time from MinInstabilityTimerSeconds to MaxInstabilityTimerSeconds seconds until bad thing
            var durationSeconds = _random.Next(MinInstabilityTimerSeconds, MaxInstabilityTimerSeconds);
            pending.EndTime = _timing.CurTime + TimeSpan.FromSeconds(durationSeconds);
        }
        else if (old >= InstabilityMutationThreshold && newVal < InstabilityMutationThreshold)
        {
            // Cancel cooldown if Instability fell back below InstabilityMutationThreshold
            if (RemComp<PendingInstabilityMutationComponent>(uid))
            {
                _popup.PopupEntity(Loc.GetString("genetics-instability-cancelled"), uid, uid);
            }
        }

        // Steady cellular damage above InstabilityDamageThreshold
        if (old <= InstabilityDamageThreshold && newVal > InstabilityDamageThreshold)
            EnsureComp<GeneticsInstabilityDamageComponent>(uid);
        else if (old > InstabilityDamageThreshold && newVal <= InstabilityDamageThreshold)
            RemComp<GeneticsInstabilityDamageComponent>(uid);
    }

    public bool CanEntityReceiveMutation(EntityUid uid, GeneticMutationPrototype proto, bool isRandom = true)
    {
        var protoId = MetaData(uid).EntityPrototype?.ID ?? "Unknown";

        if (proto.StrictEntityWhitelist != null && proto.StrictEntityWhitelist.Count > 0)
        {
            if (!IsPrototypeOrParentInList(protoId, proto.StrictEntityWhitelist))
                return false;
        }

        if (proto.StrictEntityBlacklist != null && proto.StrictEntityBlacklist.Count > 0)
        {
            if (IsPrototypeOrParentInList(protoId, proto.StrictEntityBlacklist))
                return false;
        }

        // No random hidden mutations
        if (isRandom && proto.Hidden)
            return false;

        // Random-only rules (whitelist/blacklist) only apply when generating random mutations
        if (isRandom)
        {
            // Random whitelist (if set, only select IDs can get mutation randomly)
            if (proto.EntityWhitelist != null && proto.EntityWhitelist.Count > 0)
            {
                if (!IsPrototypeOrParentInList(protoId, proto.EntityWhitelist))
                    return false;
            }

            // Random blacklist (never get mutation randomly)
            if (proto.EntityBlacklist != null && proto.EntityBlacklist.Count > 0)
            {
                if (IsPrototypeOrParentInList(protoId, proto.EntityBlacklist))
                    return false;
            }
        }

        return true;
    }

    // TODO: There's probably a better way to do this in a system somewhere
    // Recursive method. Won't scale very well.
    private bool IsPrototypeOrParentInList(string entityProtoId, IReadOnlyList<string> list)
    {
        if (list.Contains(entityProtoId))
            return true;

        if (!_proto.TryIndex<EntityPrototype>(entityProtoId, out var proto))
            return false;

        return proto.Parents is { } parents && parents.Any(parent => IsPrototypeOrParentInList(parent, list));
    }

    public bool TryModifyMutationSequence(EntityUid uid, GeneticsComponent component, string mutationId, int index, char newBase)
    {
        var entryIndex = component.Mutations.FindIndex(m => m.Id == mutationId);
        if (entryIndex == -1)
            return false;

        var entry = component.Mutations[entryIndex];

        var proto = _proto.Index<GeneticMutationPrototype>(mutationId);
        if (proto.SequencerResistant)
            return false;

        if (index < 0 || index >= entry.RevealedSequence.Length)
            return false;

        var newSeq = entry.RevealedSequence.ToCharArray();
        newSeq[index] = char.ToUpper(newBase);
        var newSeqStr = new string(newSeq);

        component.Mutations[entryIndex] = entry with { RevealedSequence = newSeqStr };
        return true;
    }

    private MutationEntry CreateMutationEntry(string mutationId, GeneticMutationPrototype proto, int block, string originalSequence,
                                              string revealedSequence, bool enabled)
    {
        return new MutationEntry(
            Block: block,
            Id: mutationId,
            Name: proto.Name,
            OriginalSequence: originalSequence,
            RevealedSequence: revealedSequence,
            Enabled: enabled,
            Description: proto.Description,
            Instability: proto.Instability,
            Conflicts: proto.Conflicts
        );
    }

    public void ScrambleDna(EntityUid uid, GeneticsComponent genetics)
    {
        // Collect all scramble-resistant mutations
        var preservedEntries = new List<MutationEntry>();

        foreach (var entry in genetics.Mutations)
        {
            if (!_proto.TryIndex<GeneticMutationPrototype>(entry.Id, out var proto))
                continue;

            if (proto.ScrambleResistant)
                preservedEntries.Add(entry);
        }

        // Remove all non-resistant active mutations
        foreach (var entry in genetics.Mutations.ToList())
        {
            if (!entry.Enabled)
                continue;

            if (!_proto.TryIndex<GeneticMutationPrototype>(entry.Id, out var proto) || !proto.ScrambleResistant)
            {
                TryDeactivateMutation(uid, genetics, entry.Id);
            }
        }

        genetics.Mutations.Clear();
        genetics.BaseMutationIds.Clear();

        genetics.GeneticInstability = 0;

        FillBaseMutations(uid, genetics);

        // Restore preserved mutations
        foreach (var preserved in preservedEntries)
        {
            if (TryAddMutation(uid, genetics, preserved.Id))
            {
                var index = genetics.Mutations.FindIndex(m => m.Id == preserved.Id);
                if (index != -1)
                {
                    var newEntry = genetics.Mutations[index];

                    genetics.Mutations[index] = newEntry with
                    {
                        RevealedSequence = preserved.RevealedSequence,
                        Enabled = preserved.Enabled
                    };
                }
            }
        }

        return;
    }
}
