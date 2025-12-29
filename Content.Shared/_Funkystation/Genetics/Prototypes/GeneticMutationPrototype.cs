using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Genetics.Prototypes;

/// <summary>
///     Defines a genetic mutation that can be discovered, activated, or suppressed via genetics machinery.
/// </summary>
[Prototype("geneticMutation")]
public sealed partial class GeneticMutationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Name shown to players once the mutation is identified.
    /// </summary>
    [DataField("name"), ViewVariables(VVAccess.ReadWrite)]
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    ///     Description shown in the DNA scanner console.
    /// </summary>
    [DataField("desc")]
    public string? Description { get; private set; }

    /// <summary>
    ///     How much the mutation modifies the entities genetic stability.
    /// </summary>
    [DataField("instability")]
    public int Instability { get; private set; } = 0;

    /// <summary>
    ///     If true, this mutation cannot randomly appear in the round.
    /// </summary>
    [DataField("hidden")]
    public bool Hidden { get; private set; } = false;

    /// <summary>
    /// If set, mutation will only be randomly applied to these entity prototype IDs or it's children.
    /// Mutation can still be applied outside of this list with a mutator.
    /// Examples: "MobHuman", "MobLizard", "MobMothroach", "MobSlime", "MobMonkey"
    /// </summary>
    [DataField("entityWhitelist")]
    public List<string>? EntityWhitelist { get; private set; }

    /// <summary>
    /// Mutation will not be randomly applied to these entity prototype IDs.
    /// </summary>
    [DataField("entityBlacklist")]
    public List<string>? EntityBlacklist { get; private set; }

    /// <summary>
    /// If set, mutation can ONLY be applied to these entity prototype IDs or it's children.
    /// Mutation cannot be applied outside of this list even if a mutator is used.
    /// </summary>
    [DataField("strictEntityWhitelist")]
    public List<string>? StrictEntityWhitelist { get; private set; }

    /// <summary>
    /// Mutation can NEVER be applied on these entity prototype IDs, even with a mutator.
    /// </summary>
    [DataField("strictEntityBlacklist")]
    public List<string>? StrictEntityBlacklist { get; private set; }

    /// <summary>
    ///     Mutations that cannot co-exist with this one (e.g. Dwarfism vs Gigantism).
    /// </summary>
    [DataField("conflicts")]
    public List<string> Conflicts { get; private set; } = new();

    /// <summary>
    /// Component ID this mutation applies when activated
    /// </summary>
    [DataField("components")]
    public ComponentRegistry Components { get; private set; } = new();

    /// <summary>
    /// If true, this mutation cannot be removed by Mutadone.
    /// </summary>
    [DataField("mutadoneResistant")]
    public bool MutadoneResistant { get; private set; } = false;

    /// <summary>
    /// If true, this mutation cannot be removed by the DNA Sequencer.
    /// </summary>
    [DataField("sequencerResistant")]
    public bool SequencerResistant { get; private set; } = false;

    /// <summary>
    /// If true, this mutation cannot be removed via DNA scramble.
    /// </summary>
    [DataField("scrambleResistant")]
    public bool ScrambleResistant { get; private set; } = false;

    /// <summary>
    /// If true, this mutation can be printed as a genetic injector.
    /// </summary>
    [DataField("printable")]
    public bool Printable { get; private set; } = true;

    /// <summary>
    /// If true, this mutation will be added to the pool of random negative mutations
    /// devoloped from exceeding 100 genetic instability.
    /// </summary>
    [DataField("instabilityMutation")]
    public bool InstabilityMutation { get; private set; } = false;

    /// <summary>
    /// Custom popup text shown to the player when this mutation is activated.
    /// </summary>
    [DataField("popupText")]
    public string? PopupText { get; private set; }

    /// <summary>
    ///     Weight for random selection when radiation triggers a mutation.
    ///     Higher = more likely to be picked. Default is 10.
    /// </summary>
    [DataField("probabilityWeight")]
    public float ProbabilityWeight { get; private set; } = 10f;

    /// <summary>
    ///     Research points granted to the station's research server the first time this mutation is discovered.
    /// </summary>
    [DataField("researchPoints")]
    public int ResearchPoints { get; private set; } = 300;
}
