using Content.Shared._Funkystation.Genetics.Components;

namespace Content.Shared._Funkystation.Genetics.Systems;

public sealed class SharedMutationDiscoverySystem : EntitySystem
{
    public void DiscoverMutation(EntityUid consoleUid, string mutationId)
    {
        var tracker = GetGridTracker(consoleUid);
        tracker?.GridDiscoveredMutations.Add(mutationId);
    }

    public HashSet<string> GetGridDiscovered(EntityUid consoleUid)
    {
        if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not { } gridUid)
            return new();

        return TryComp<DnaScannerDiscoveryTrackerComponent>(gridUid, out var tracker)
            ? new HashSet<string>(tracker.GridDiscoveredMutations)
            : new();
    }

    private DnaScannerDiscoveryTrackerComponent? GetGridTracker(EntityUid consoleUid)
    {
        if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not { } gridUid)
            return null;

        return CompOrNull<DnaScannerDiscoveryTrackerComponent>(gridUid);
    }

    public Dictionary<string, int> GetGridResearchProgress(EntityUid consoleUid)
    {
        var tracker = GetGridTracker(consoleUid);
        return tracker?.GridResearchProgress ?? new Dictionary<string, int>();
    }

    public Dictionary<string, int> GetMutableGridResearchProgress(EntityUid consoleUid)
    {
        var tracker = GetGridTracker(consoleUid);
        if (tracker == null)
        {
            if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not { } gridUid)
                return new Dictionary<string, int>();

            tracker = EnsureComp<DnaScannerDiscoveryTrackerComponent>(gridUid);
        }

        return tracker.GridResearchProgress;
    }

    public void DeductResearchProgress(EntityUid consoleUid, string mutationId, int deductAmount)
    {
        var tracker = GetGridTracker(consoleUid);
        if (tracker == null)
        {
            if (!TryComp<TransformComponent>(consoleUid, out var xform) || xform.GridUid is not EntityUid gridUid)
                return;

            tracker = EnsureComp<DnaScannerDiscoveryTrackerComponent>(gridUid);
        }

        if (tracker.GridResearchProgress.TryGetValue(mutationId, out var current))
        {
            tracker.GridResearchProgress[mutationId] -= deductAmount;

            if (tracker.GridResearchProgress[mutationId] < 0)
                tracker.GridResearchProgress[mutationId] = 0;
        }
    }
}
