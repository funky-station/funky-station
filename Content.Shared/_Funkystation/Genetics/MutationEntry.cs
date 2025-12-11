using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Genetics;

[Serializable, NetSerializable]
public sealed record MutationEntry(
    int Block,
    string Id,
    string Name,
    string OriginalSequence,
    string RevealedSequence,
    bool Enabled,
    string? Description = null,
    int Instability = 0,
    IReadOnlyList<string>? Conflicts = null);
