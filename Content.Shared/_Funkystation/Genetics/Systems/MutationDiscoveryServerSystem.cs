using Content.Shared._Funkystation.Genetics.Components;

namespace Content.Shared._Funkystation.Genetics.Systems;

public sealed class SharedMutationDiscoverySystem : EntitySystem
{
    public void DiscoverMutation(EntityUid consoleUid, string mutationId)
    {
        if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not { } gridUid)
            return;

        var tracker = EnsureComp<DnaScannerDiscoveryTrackerComponent>(gridUid);
        tracker.GridDiscoveredMutations.Add(mutationId);
    }

    public HashSet<string> GetGridDiscovered(EntityUid consoleUid)
    {
        if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not { } gridUid)
            return new();

        return TryComp<DnaScannerDiscoveryTrackerComponent>(gridUid, out var tracker)
            ? new HashSet<string>(tracker.GridDiscoveredMutations)
            : new();
    }
}
