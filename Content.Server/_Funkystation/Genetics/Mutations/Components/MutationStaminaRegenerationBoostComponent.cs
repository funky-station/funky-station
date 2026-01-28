using Robust.Shared.GameStates;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationStaminaRegenerationBoostComponent : Component
{
    [DataField]
    public float RegenBonus = 1.5f;
}
