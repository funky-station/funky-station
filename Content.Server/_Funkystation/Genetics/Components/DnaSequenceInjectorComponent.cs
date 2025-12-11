namespace Content.Server._Funkystation.Genetics.Components;

[RegisterComponent]
public sealed partial class DnaSequenceInjectorComponent : Component
{
    /// <summary>
    /// Mutation prototype ID to inject
    /// </summary>
    [DataField]
    public string? MutationId;

    /// <summary>
    /// Is the injector a mutator or an activator
    /// </summary>
    [DataField]
    public bool IsMutator = false;
}
