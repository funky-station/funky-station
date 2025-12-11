using System.Collections.Frozen;

namespace Content.Server._Funkystation.Genetics.Components;

[RegisterComponent]
public sealed partial class GeneticShuffleSingletonComponent : Component
{
    /// <summary>
    ///   The shuffled mutations for the current round.
    ///   Immutable after round start.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public IReadOnlyDictionary<string, GeneticBlock> Mutation { get; private set; } = FrozenDictionary<string, GeneticBlock>.Empty;

    /// <summary>
    ///   Called only by GeneticShuffleSystem during round start.
    /// </summary>
    internal void SetMutation(Dictionary<string, GeneticBlock> mutation)
    {
        Mutation = mutation.ToFrozenDictionary();
    }

    /// <summary>
    ///   Resets for next round / lobby.
    /// </summary>
    internal void Clear()
    {
        Mutation = FrozenDictionary<string, GeneticBlock>.Empty;
    }
}
