using Content.Shared._Funkystation.Genetics;

namespace Content.Server._Funkystation.Genetics.Components;

[RegisterComponent]
[AutoGenerateComponentState(true)]
public sealed partial class GeneticsComponent : Component
{
    [DataField("slots"), ViewVariables(VVAccess.ReadWrite)]
    public int MutationSlots { get; private set; } = 6;

    /// <summary>
    /// All mutations this entity currently has (active or not).
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public List<MutationEntry> Mutations { get; } = new();

    /// <summary>
    /// IDs of mutations that were part of the original genome (don't cause instability).
    /// </summary>
    [DataField]
    public HashSet<string> BaseMutationIds { get; set; } = new();

    [DataField("baseMutations")]
    public List<ForcedMutation> ForcedBaseMutations { get; private set; } = new();

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public int GeneticInstability { get; set; } = 0;

    [ViewVariables(VVAccess.ReadWrite)]
    [AutoNetworkedField]
    public float RadsUntilRandomMutation { get; set; } = 50f;
}

[Serializable]
[DataDefinition]
public sealed partial class ForcedMutation
{
    [DataField("id", required: true)]
    public string Id { get; set; } = default!;

    [DataField("startActive")]
    public bool StartActive { get; set; } = false;
}

