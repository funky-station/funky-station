using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
///     Exists for use as a mutation status effect.
///     Adds the DamageProtectionBuffComponent to the entity and adds the specified DamageModifierSet to its list of modifiers.
/// </summary>
/// <remarks>
///     This has been copied from RadiationProtectionComponent to prevent component conflicts with mutation enabling/disabling
/// </remarks>
[RegisterComponent]
public sealed partial class MutationRadiationResistanceComponent : Component
{
    /// <summary>
    ///     The damage modifier set that reduces radiation damage.
    /// </summary>
    [DataField("modifier")]
    public ProtoId<DamageModifierSetPrototype> ModifierSetId = "RadiationResistance";
}
