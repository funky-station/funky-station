using Content.Server._Funkystation.Genetics.Mutations.Systems;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationThermalResistanceComponent : Component
{
    /// <summary>
    /// Multiplier for heat transfer when heating up (higher = worse insulation, lets more heat in)
    /// </summary>
    [DataField]
    public float HeatingCoefficient = 1.0f;

    /// <summary>
    /// Multiplier for heat transfer when cooling down
    /// </summary>
    [DataField]
    public float CoolingCoefficient = 1.0f;
}
