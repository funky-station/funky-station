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

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed partial class GeneticsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GeneticShuffleSystem _shuffle = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticsComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<GeneticsComponent, DamageChangedEvent>(OnRadiationDamage);
    }

    private void OnInit(EntityUid uid, GeneticsComponent component, ComponentInit args)
    {
        FillBaseMutations(uid, component);
        component.RadsUntilRandomMutation = _random.NextFloat(15f, 80f);
        Dirty(uid, component);
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
        component.RadsUntilRandomMutation = _random.NextFloat(15f, 80f);
        Dirty(uid, component);
    }

    public void FillBaseMutations(EntityUid uid, GeneticsComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        component.Mutations.Clear();
        component.BaseMutationIds.Clear();

        var added = 0;

        // Apply forced base mutations
        foreach (var forced in component.ForcedBaseMutations)
        {
            if (!_proto.TryIndex<GeneticMutationPrototype>(forced.Id, out var proto))
                continue;

            if (!CanEntityReceiveMutation(uid, proto))
                continue;

            if (!_shuffle.TryGetSlot(forced.Id, out var slot))
                continue;

            var revealed = forced.StartActive ? slot.Sequence : RandomizeSequence(slot.Sequence);
            var entry = CreateMutationEntry(
                mutationId: forced.Id,
                proto: proto,
                block: slot.Block,
                originalSequence: slot.Sequence,
                revealedSequence: revealed,
                enabled: forced.StartActive
            );

            component.Mutations.Add(entry);
            component.BaseMutationIds.Add(forced.Id);

            // Apply components if it starts active. Skips all checks.
            if (forced.StartActive)
                ApplyMutationComponents(uid, component, proto);

            added++;
        }

        var slotsLeft = Math.Max(0, component.MutationSlots - added);

        if (slotsLeft <= 0)
        {
            Dirty(uid, component);
            return;
        }

        // Fill random mutations
        var available = _shuffle.CurrentMutation()
            .Where(x => x.Value.Block > 0)
            .Where(x => !component.BaseMutationIds.Contains(x.Key))
            .ToList();

        if (available.Count == 0)
            return;

        _random.Shuffle(available);
        var toAdd = Math.Min(component.MutationSlots, available.Count);

        for (var i = 0; i < toAdd; i++)
        {
            var (id, slot) = available[i];
            if (!_proto.TryIndex(id, out GeneticMutationPrototype? proto))
                continue;

            if (!CanEntityReceiveMutation(uid, proto))
                continue;

            var revealed = RandomizeSequence(slot.Sequence);
            var entry = CreateMutationEntry(
                mutationId: id,
                proto: proto,
                block: slot.Block,
                originalSequence: slot.Sequence,
                revealedSequence: revealed,
                enabled: false
            );

            component.Mutations.Add(entry);
            component.BaseMutationIds.Add(id);
        }

        if (TryComp<MetaDataComponent>(uid, out var meta))
            Dirty(uid, meta);
    }

    public void TriggerRandomMutation(EntityUid uid, GeneticsComponent component)
    {
        var available = _shuffle.CurrentMutation()
            .Where(x => x.Value.Block > 0)
            .Select(x => x.Key)
            .Where(id => _proto.TryIndex(id, out GeneticMutationPrototype? proto) && CanEntityReceiveMutation(uid, proto))
            .ToList();

        if (available.Count == 0)
            return;

        var chosen = _random.Pick(available);
        TryAddMutation(uid, component, chosen);
        TryActivateMutation(uid, component, chosen);
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

        var revealCount = _random.Next((int) (length * 0.6f), (int) (length * 0.85f));
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
        if (!_shuffle.TryGetSlot(mutationId, out var slot) ||
            component.Mutations.Any(m => m.Id == mutationId) ||
            !_proto.TryIndex(mutationId, out GeneticMutationPrototype? proto))
            return false;

        if (!CanEntityReceiveMutation(uid, proto, false))
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
            Dirty(uid, component);
            return true;
        }

        component.Mutations.Remove(entry);

        if (_proto.TryIndex(mutationId, out GeneticMutationPrototype? proto))
        {
            RemoveMutationComponents(uid, proto);
        }

        Dirty(uid, component);
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
        Dirty(uid, component);
    }

    private void RemoveMutationComponents(EntityUid uid, GeneticMutationPrototype proto)
    {
        foreach (var (comp, _) in proto.Components.Values)
        {
            if (HasComp(uid, comp.GetType()))
                RemComp(uid, comp.GetType());
        }
    }

    private void ModifyInstability(EntityUid uid, GeneticsComponent component, int delta)
    {
        if (delta == 0) return;

        var old = component.GeneticInstability;
        component.GeneticInstability += delta;
        var newVal = component.GeneticInstability;

        // Bad things happen past 100
        if (old <= 100 && newVal > 100)
        {
            // Remove any existing pending timer
            RemComp<PendingInstabilityMutationComponent>(uid);

            var pending = AddComp<PendingInstabilityMutationComponent>(uid);
            pending.MutationId = string.Empty;

            // Set a random time from 60 to 90 seconds until bad thing
            var durationSeconds = _random.Next(60, 90);
            pending.EndTime = _timing.CurTime + TimeSpan.FromSeconds(durationSeconds);
        }
        else if (old >= 100 && newVal < 100)
        {
            // Cancel cooldown if Instability fell back below 100
            if (RemComp<PendingInstabilityMutationComponent>(uid))
            {
                _popup.PopupEntity(Loc.GetString("genetics-instability-cancelled"), uid, uid);
            }
        }

        // Steady cellular damage above 150
        if (old <= 150 && newVal > 150)
            EnsureComp<GeneticsInstabilityDamageComponent>(uid);
        else if (old > 150 && newVal <= 150)
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
        Dirty(uid, component);
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

        Dirty(uid, genetics);
        return;
    }
}
