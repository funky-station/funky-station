using Content.Shared._Funkystation.Genetics.Systems;

namespace Content.Shared._Funkystation.Genetics.Components;

[RegisterComponent, Access(typeof(SharedMutationDiscoverySystem))]
public sealed partial class DnaScannerDiscoveryTrackerComponent : Component
{
    [DataField] public HashSet<string> GridDiscoveredMutations { get; private set; } = new();

    /// <summary>
    /// Persistent tracking of research progress for all mutations that have ever been queued.
    /// </summary>
    [DataField]
    public Dictionary<string, int> GridResearchProgress = new();
}
