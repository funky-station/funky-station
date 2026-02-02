using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Added directly to a mob to provide passive armor.
/// </summary>
[RegisterComponent]
public sealed partial class MutationPassiveArmorProviderComponent : Component
{
    /// <summary>
    /// The damage modifiers provided by this mutation.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<DamageModifierSetPrototype> ModifierSetId = default!;
}
