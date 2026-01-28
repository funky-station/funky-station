using Content.Shared._Funkystation.Genetics;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Temporary data holder that survives polymorph to restore genetics state.
/// </summary>
[RegisterComponent]
public sealed partial class PolymorphGeneticsRestoreDataComponent : Component
{
    public List<MutationEntry> MutationSnapshot = new();
    public HashSet<string> EnabledMutationIds = new();
    public int GeneticInstability;
    public HashSet<string> BaseMutationIds = new();
}
