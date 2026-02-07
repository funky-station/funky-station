namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationParanoiaComponent : Component
{
    [DataField] public float Interval = 60.0f;
    [DataField] public float EmoteChance = 0.6f;
    [ViewVariables] public TimeSpan NextCheck;
}
