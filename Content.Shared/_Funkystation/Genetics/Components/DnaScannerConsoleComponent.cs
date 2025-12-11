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
    /// Number of available DNA injectors in storage
    /// </summary>
    [DataField] public int DnaInjectors = 60;

    /// <summary>
    /// When the scramble cooldown ends
    /// </summary>
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan? ScrambleCooldownEnd;

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
public sealed class DnaScannerConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public string? SubjectName { get; init; }
    public string? HealthStatus { get; init; }
    public float? GeneticDamage { get; init; }
    public int SubjectGeneticInstability { get; init; }
    public TimeSpan? ScrambleCooldownEnd { get; init; }
    public List<MutationEntry>? Mutations { get; init; }
    public HashSet<string> DiscoveredMutationIds { get; init; } = new();
    public HashSet<string> BaseMutationIds { get; init; } = new();
    public List<MutationEntry> SavedMutations { get; init; } = new();
    public bool IsFullUpdate { get; init; }

    public DnaScannerConsoleBoundUserInterfaceState(
        string? subjectName = null,
        string? healthStatus = null,
        float? geneticDamage = null,
        int subjectGeneticInstability = 0,
        TimeSpan? scrambleCooldownEnd = null,
        List<MutationEntry>? mutations = null,
        HashSet<string>? discoveredMutationIds = null,
        HashSet<string>? baseMutationIds = null,
        List<MutationEntry>? savedMutations = null,
        bool isFullUpdate = true)
    {
        SubjectName = subjectName;
        HealthStatus = healthStatus;
        GeneticDamage = geneticDamage;
        SubjectGeneticInstability = subjectGeneticInstability;
        ScrambleCooldownEnd = scrambleCooldownEnd;
        Mutations = mutations;
        DiscoveredMutationIds = discoveredMutationIds ?? new();
        BaseMutationIds = baseMutationIds ?? new();
        SavedMutations = savedMutations ?? new();
        IsFullUpdate = isFullUpdate;
    }
}

[Serializable, NetSerializable]
public enum DnaScannerConsoleUiKey : byte
{
    Key
}
