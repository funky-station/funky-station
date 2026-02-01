using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Funkystation.Genetics.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class DnaScannerConsoleComponent : Component
{
    /// <summary>
    /// Currently scanned entity
    /// </summary>
    [DataField] public EntityUid? CurrentSubject;

    /// <summary>
    /// Mutations saved in the console's storage
    /// </summary>
    [DataField] public List<MutationEntry> SavedMutations = new();

    /// <summary>
    /// Mutations currently being researched in this console.
    /// </summary>
    [DataField]
    public HashSet<string> ActiveResearchQueue = new();

    /// <summary>
    /// Last time we processed a research tick (once per second).
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? LastResearchTick;

    /// <summary>
    /// Number of available DNA injectors in storage
    /// </summary>
    [DataField] public int DnaInjectors = 60;

    /// <summary>
    /// When the scramble cooldown ends
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? ScrambleCooldownEnd;

    /// <summary>
    /// When the joker ability cooldown ends
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? JokerCooldownEnd;

    /// <summary>
    /// Next time to send a health tick
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? NextHealthUpdate;

    [ViewVariables(VVAccess.ReadWrite), DataField("soundDeny")]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("soundDnaScramble")]
    public SoundSpecifier SoundDnaScramble = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}

[Serializable, NetSerializable]
public sealed class GeneticistsConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public string? SubjectName { get; init; }
    public string? HealthStatus { get; init; }
    public float? RadiationDamage { get; init; }
    public int SubjectGeneticInstability { get; init; }
    public TimeSpan? ScrambleCooldownEnd { get; init; }
    public List<MutationEntry>? Mutations { get; init; }
    public HashSet<string> DiscoveredMutationIds { get; init; } = new();
    public HashSet<string> BaseMutationIds { get; init; } = new();
    public List<MutationEntry> SavedMutations { get; init; } = new();
    public bool IsFullUpdate { get; init; }
    public Dictionary<string, int> ResearchRemaining { get; init; } = new();
    public Dictionary<string, int> ResearchOriginal { get; init; } = new();
    public HashSet<string> ActiveResearchMutationIds { get; init; } = new();
    public TimeSpan? JokerCooldownEnd { get; init; }

    public GeneticistsConsoleBoundUserInterfaceState(
        string? subjectName = null,
        string? healthStatus = null,
        float? radiationDamage = null,
        int subjectGeneticInstability = 0,
        TimeSpan? scrambleCooldownEnd = null,
        List<MutationEntry>? mutations = null,
        HashSet<string>? discoveredMutationIds = null,
        HashSet<string>? baseMutationIds = null,
        List<MutationEntry>? savedMutations = null,
        bool isFullUpdate = true,
        Dictionary<string, int> researchRemaining = default!,
        Dictionary<string, int> researchOriginal = default!,
        HashSet<string>? activeResearchMutationIds = null,
        TimeSpan? jokerCooldownEnd = null)
    {
        SubjectName = subjectName;
        HealthStatus = healthStatus;
        RadiationDamage = radiationDamage;
        SubjectGeneticInstability = subjectGeneticInstability;
        ScrambleCooldownEnd = scrambleCooldownEnd;
        Mutations = mutations;
        DiscoveredMutationIds = discoveredMutationIds ?? new();
        BaseMutationIds = baseMutationIds ?? new();
        SavedMutations = savedMutations ?? new();
        IsFullUpdate = isFullUpdate;
        ResearchRemaining = researchRemaining;
        ResearchOriginal = researchOriginal;
        ActiveResearchMutationIds = activeResearchMutationIds ?? new();
        JokerCooldownEnd = jokerCooldownEnd;
    }
}

[Serializable, NetSerializable]
public enum DnaScannerConsoleUiKey : byte
{
    Key
}
