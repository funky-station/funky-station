using Content.Shared.Atmos;
using Robust.Shared.GameStates;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

/// <summary>
/// Non-physical representation of a reactor part.
/// </summary>
public abstract partial class ReactorPart : Component
{
    /// <summary>
    /// Icon of this component as it shows in the UIs.
    /// </summary>
    [DataField]
    public string IconStateInserted = "base";

    /// <summary>
    /// Icon of this component as it shows in the world.
    /// </summary>
    [DataField]
    public string IconStateCap = "rod_cap";

    #region Variables
    /// <summary>
    /// Temperature of this component, starts at room temp Kelvin by default.
    /// </summary>
    [DataField]
    public float Temperature = Atmospherics.T20C;

    /// <summary>
    /// How much does this component share heat with surrounding components? Basically surface area in contact (m2).
    /// </summary>
    [DataField]
    public float ThermalCrossSection = 10;

    /// <summary>
    /// How adept is this component at interacting with neutrons - fuel rods are set up to capture them, heat exchangers are set up not to.
    /// </summary>
    [DataField]
    public float NeutronCrossSection = 0.5f;

    /// <summary>
    /// Control rods don't moderate neutrons, they absorb them.
    /// </summary>
    [DataField]
    public bool IsControlRod = false;

    /// <summary>
    /// Max health to set <see cref="MeltHealth"/> to on init.
    /// </summary>
    [DataField]
    public float MaxHealth = 100;

    /// <summary>
    /// Essentially indicates how long this component can be at a dangerous temperature before it melts.
    /// </summary>
    [DataField]
    public float MeltHealth = 100;

    /// <summary>
    /// If this component is melted, you can't take it out of the reactor and it might do some weird stuff.
    /// </summary>
    [DataField]
    public bool Melted = false;

    /// <summary>
    /// The dangerous temperature above which this component starts to melt. 1700K is the melting point of steel.
    /// </summary>
    [DataField]
    public float MeltingPoint = 1700;

    /// <summary>
    /// How much gas this component can hold, and will be processed per tick.
    /// </summary>
    [DataField]
    public float GasVolume = 0;

    /// <summary>
    /// Thermal mass. Basically how much energy it takes to heat this up 1Kelvin.
    /// </summary>
    [DataField]
    public float ThermalMass = 420 * 250; //specific heat capacity of steel (420 J/KgK) * mass of component (Kg)
    #endregion

    #region Properties
    // SS13 material properties for Steel
    /// <summary>
    /// The material density of the rod. Determines how likley it is to interact with neutrons.
    /// </summary>
    [DataField]
    public float PropertyDensity = 4;

    /// <summary>
    /// The thermal conductivity of the rod. Determines the rate of heat transfer.
    /// </summary>
    [DataField]
    public float PropertyThermal = 7; //was 6

    /// <summary>
    /// The material hardness of the rod. Determines how likley it is to reflect neutrons.
    /// </summary>
    [DataField]
    public float PropertyHard = 3;

    /// <summary>
    /// Neutron radioactivity, basically how much fuel is in the rod.
    /// </summary>
    public float NRadioactive = 0;

    /// <summary>
    /// Radioactivity.
    /// </summary>
    public float Radioactive = 0;

    /// <summary>
    /// How much spent fuel is in the rod.
    /// </summary>
    public float SpentFuel = 0;
    #endregion
}

/// <summary>
/// A reactor part for the reactor grid.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorPartComponent : ReactorPart;

/// <summary>
/// A virtual neutron that flies around within the reactor.
/// </summary>
[NetworkedComponent]
public sealed class ReactorNeutron
{
    public Direction dir = Direction.North;
    public float velocity = 1;
}

/// <summary>
/// A control rod for the reactor grid.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorControlRodComponent : ReactorPart
{
    /// <summary>
    /// The target insertion level of the control rod.
    /// </summary>
    [DataField]
    public float ConfiguredInsertionLevel = 1;
}

/// <summary>
/// A gas channel for the reactor grid.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorGasChannelComponent : ReactorPart
{
    /// <summary>
    /// How adept the gas channel is at transfering heat to/from gasses.
    /// </summary>
    [DataField]
    public float GasThermalCrossSection = 25; //was 15

    /// <summary>
    /// The gas mixture inside the gas channel.
    /// </summary>
    public GasMixture? AirContents;
}

[NetworkedComponent]
public static class BaseReactorComponents
{
    public static readonly ReactorControlRodComponent ControlRod = new()
    {
        IconStateInserted = "control",
        IconStateCap = "control_cap",
        IsControlRod = true,
        NeutronCrossSection = 1.0f,
        ThermalCrossSection = 10,
        PropertyDensity = 6,
        PropertyHard = 5,
        Temperature = default,
    };

    public static readonly ReactorPartComponent FuelRod = new()
    {
        IconStateInserted = "fuel",
        IconStateCap = "fuel_cap",
        NeutronCrossSection = 1.0f,
        ThermalCrossSection = 10,
        ThermalMass = 420000,
        NRadioactive = 1.5f,
        Radioactive = 4,
        PropertyHard = 5,
        PropertyDensity = 7,
        PropertyThermal = 4,
        Temperature = default,
    };

    public static readonly ReactorGasChannelComponent GasChannel = new()
    {
        IconStateInserted = "gas",
        IconStateCap = "gas_cap",
        ThermalCrossSection = 15,
        GasVolume = 100,
        ThermalMass = 21000,
        Temperature = default,
    };

    public static readonly ReactorPartComponent HeatExchanger = new()
    {
        IconStateInserted = "heat",
        IconStateCap = "heat_cap",
        NeutronCrossSection = 0.1f,
        ThermalCrossSection = 25,
        Temperature = default,
    };
}