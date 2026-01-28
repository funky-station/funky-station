namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationStomachSwapComponent : Component
{
    [DataField(required: true)]
    public string NewStomachPrototype = default!;

    public EntityUid? OriginalStomach { get; set; }
    public EntityUid? SwappedStomach { get; set; }
}
