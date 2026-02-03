using System.Linq;
using Content.Server._Funkystation.Genetics.Systems;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Genetics.Systems;

public sealed class MutationUnlockTriggerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly GeneticShuffleSystem _shuffle = default!;
    [Dependency] private readonly SharedMutationDiscoverySystem _discovery = default!;

    private readonly List<MutationUnlockTriggerPrototype> _triggers = new();

    public override void Initialize()
    {
        base.Initialize();
        _proto.PrototypesReloaded += OnPrototypesReloaded;
        LoadTriggers();
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        if (args.ByType.ContainsKey(typeof(MutationUnlockTriggerPrototype)))
            LoadTriggers();
    }

    private void LoadTriggers()
    {
        _triggers.Clear();
        _triggers.AddRange(_proto.EnumeratePrototypes<MutationUnlockTriggerPrototype>());
    }

    /// <summary>
    /// Call this every time a mutation is successfully saved to a console.
    /// </summary>
    public void OnMutationSaved(EntityUid consoleUid, DnaScannerConsoleComponent console, string savedMutationId)
    {
        var savedIds = console.SavedMutations.Select(m => m.Id).ToHashSet();

        foreach (var trigger in _triggers)
        {
            // Check if all required mutations are now saved
            if (!trigger.RequiredMutations.All(req => savedIds.Contains(req)))
                continue;

            // Unlock all specified mutations
            foreach (var unlockId in trigger.UnlockMutations)
            {
                if (console.SavedMutations.Any(m => m.Id == unlockId))
                    continue;

                if (!_proto.TryIndex<GeneticMutationPrototype>(unlockId, out var proto))
                    continue;

                var slot = _shuffle.GetOrAssignSlot(unlockId);

                if (slot.Block <= 0)
                    continue;

                var entry = new MutationEntry(
                    Block: slot.Block,
                    Id: unlockId,
                    Name: proto.Name,
                    OriginalSequence: slot.Sequence,
                    RevealedSequence: slot.Sequence,
                    Enabled: false,
                    Description: proto.Description,
                    Instability: proto.Instability,
                    Conflicts: proto.Conflicts
                );

                // Also discover it on the grid
                _discovery.DiscoverMutation(consoleUid, unlockId);
            }
        }

        if (console.SavedMutations.Count != savedIds.Count)
            Dirty(consoleUid, console);
    }
}
