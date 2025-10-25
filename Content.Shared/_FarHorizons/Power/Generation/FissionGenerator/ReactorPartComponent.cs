using Content.Shared.Atmos;
using Robust.Shared.GameStates;
using Robust.Shared.Random;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract partial class ReactorPart : Component
{
    [DataField]
    public string IconStateInserted = "base";
    [DataField]
    public string IconStateCap = "rod_cap";

    #region Variables
    /// <summary>
    /// Temperature of this component, starts at room temp Kelvin by default
    /// </summary>
    [DataField]
    public float Temperature = Atmospherics.T20C;

    /// <summary>
    /// How much does this component share heat with surrounding components? Basically surface area in contact (m2)
    /// </summary>
    [DataField]
    public float ThermalCrossSection = 10;

    /// <summary>
    /// How adept is this component at interacting with neutrons - fuel rods are set up to capture them, heat exchangers are set up not to
    /// </summary>
    [DataField]
    public float NeutronCrossSection = 0.5f;

    /// <summary>
    /// Control rods don't moderate neutrons, they absorb them.
    /// </summary>
    [DataField]
    public bool IsControlRod = false;

    /// <summary>
    /// Max health to set melt_health to on init
    /// </summary>
    [DataField]
    public float MaxHealth = 100;

    /// <summary>
    /// Essentially indicates how long this component can be at a dangerous temperature before it melts
    /// </summary>
    [DataField]
    public float MeltHealth = 100;

    /// <summary>
    /// If this component is melted, you can't take it out of the reactor and it might do some weird stuff
    /// </summary>
    [DataField]
    public bool Melted = false;

    /// <summary>
    /// The dangerous temperature above which this component starts to melt. 1700K is the melting point of steel
    /// </summary>
    [DataField]
    public float MeltingPoint = 1700;

    /// <summary>
    /// How much gas this component can hold, and will be processed per tick
    /// </summary>
    [DataField]
    public float GasVolume = 0;

    /// <summary>
    /// Thermal mass. Basically how much energy it takes to heat this up 1Kelvin
    /// </summary>
    [DataField]
    public float ThermalMass = 420 * 250; //specific heat capacity of steel (420 J/KgK) * mass of component (Kg)

    public FissionGeneratorComponent? ParentReactor;
    #endregion

    #region Properties
    // SS13 material properties for Steel
    public readonly float _propertyDensity = 4;
    public readonly float _propertyThermal = 6;
    public readonly float _propertyHard = 3;

    /// <summary>
    /// Neutron radioactivity, basically how much fuel is in the rod
    /// </summary>
    public float NRadioactive = 0;

    /// <summary>
    /// Radioactivity
    /// </summary>
    public float Radioactive = 0;

    /// <summary>
    /// How much spent fuel is in the rod
    /// </summary>
    public float SpentFuel = 0;
    #endregion

    public void AssignParentReactor(FissionGeneratorComponent reactor) => ParentReactor = reactor;
    public void RemoveParentReactor() => ParentReactor = null;

    public virtual void Melt(IRobustRandom random)
    {
        if (Melted)
            return;

        Melted = true;
        IconStateCap += "_melted_" + random.Next(1, 4);
        // TODO: tell parent to update its looks
        NeutronCrossSection = 5f;
        ThermalCrossSection = 20f;
        IsControlRod = false;
    }

    public void ProcessHeat(List<ReactorPart?> AdjacentComponents, IRobustRandom random)
    {
        // Intercomponent calculation
        foreach (var RC in AdjacentComponents)
        {
            if (RC == null)
                continue;

            var DeltaT = Temperature - RC.Temperature;
            // The thermal conductivity for a steel/steel interaction in SS13. SS14 does not support material properties
            // like this, so this is the best I can do
            var k = (Math.Pow(10, _propertyThermal / 5) - 1 + (Math.Pow(10, _propertyThermal / 5) - 1)) / 2;
            var A = Math.Min(ThermalCrossSection, RC.ThermalCrossSection);

            Temperature = (float)(Temperature - (k * A * (0.4 * 8) / ThermalMass * DeltaT));
            RC.Temperature = (float)(RC.Temperature - (k * A * (0.4 * 8) / RC.ThermalMass * -DeltaT));

            if (RC.Temperature < 0 || Temperature < 0)
                return; // TODO: crash the game here

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }

        // Component-Reactor calculation
        if (ParentReactor != null)
        {
            var DeltaT = Temperature - ParentReactor.Temperature;

            var k = (Math.Pow(10, _propertyThermal / 5) - 1 + (Math.Pow(10, _propertyThermal / 5) - 1)) / 2;
            var A = ThermalCrossSection;

            Temperature = (float)(Temperature - (k * A * (0.4 * 8) / ThermalMass * DeltaT));
            ParentReactor.Temperature = (float)(ParentReactor.Temperature - (k * A * (0.4 * 8) / ParentReactor.ThermalMass * -DeltaT));

            if (ParentReactor.Temperature < 0 || Temperature < 0)
                return; // TODO: crash the game here

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }
        if (Temperature > MeltingPoint && MeltHealth > 0)
            MeltHealth -= random.Next(10, 50);
        if (MeltHealth <= 0)
            Melt(random);
    }

    public virtual List<ReactorNeutron> ProcessNeutrons(List<ReactorNeutron> neutrons, IRobustRandom random)
    {
        // Why not use a foreach? It doesn't work. Don't ask why, it just doesn't.
        var flux = neutrons;
        for (var n = 0; n < flux.Count; n++)
        {
            var neutron = flux[n];

            // The "4" is SS13's definition of steel's density
            if (Prob(_propertyDensity * 10 * NeutronCrossSection, random))
            {
                if (neutron.velocity <= 1 && Prob(10 * NRadioactive, random)) // neutron stimulated emission
                {
                    NRadioactive -= 0.01f;
                    Radioactive += 0.005f;
                    for (var i = 0; i < random.Next(1, 5); i++)
                    {
                        neutrons.Add(new() { dir = random.NextAngle().GetDir(), velocity = random.Next(2, 3) });
                    }
                    neutrons.Remove(neutron);
                    Temperature += 50;
                }
                else if (neutron.velocity <= 1 && Prob(10 * Radioactive, random)) // stimulated emission
                {
                    Radioactive -= 0.01f;
                    SpentFuel += 0.005f;
                    for (var i = 0; i < random.Next(1, 5); i++)
                    {
                        neutrons.Add(new() { dir = random.NextAngle().GetDir(), velocity = random.Next(1, 3) });
                    }
                    neutrons.Remove(neutron);
                    Temperature += 25;
                }
                else
                {
                    if (Prob(10 * _propertyHard, random)) // reflection, based on hardness
                        // A really complicated way of saying do a 180 or a 180+/-45
                        neutron.dir = (neutron.dir.GetOpposite().ToAngle() + (random.NextAngle() / 4) - (MathF.Tau / 8)).GetDir();
                    else if (IsControlRod)
                        neutron.velocity = 0;
                    else
                        neutron.velocity--;

                    if (neutron.velocity <= 0)
                        neutrons.Remove(neutron);

                    Temperature += 1;
                }
            }
        }
        if (Prob(NRadioactive * 10 * NeutronCrossSection, random))
        {
            for (var i = 0; i < random.Next(1, 3); i++)
            {
                neutrons.Add(new() { dir = random.NextAngle().GetDir(), velocity = 3 });
            }
            NRadioactive -= 0.01f;
            Radioactive += 0.005f;
            Temperature += 20;
        }
        if (Prob(Radioactive * 10 * NeutronCrossSection, random))
        {
            for (var i = 0; i < random.Next(1, 3); i++)
            {
                neutrons.Add(new() { dir = random.NextAngle().GetDir(), velocity = random.Next(1, 3) });
            }
            Radioactive -= 0.01f;
            SpentFuel += 0.005f;
            Temperature += 10;
        }

        neutrons ??= [];
        return neutrons;
    }

    /// <summary>
    /// Returns a true according to a probability
    /// </summary>
    /// <param name="probability">Double, 0-100 </param>
    /// <returns></returns>
    private static bool Prob(double probability, IRobustRandom random) => random.NextDouble() <= probability / 100;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorPartComponent : ReactorPart;

[NetworkedComponent]
public sealed class ReactorNeutron
{
    public Direction dir = Direction.North;
    public float velocity = 1;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorControlRodComponent : ReactorPart
{
    [DataField]
    public float ConfiguredInsertionLevel = 1;

    public override List<ReactorNeutron> ProcessNeutrons(List<ReactorNeutron> neutrons, IRobustRandom random)
    {
        base.ProcessNeutrons(neutrons, random);

        if(!Melted && (NeutronCrossSection != ConfiguredInsertionLevel))
        {
            if (ConfiguredInsertionLevel < NeutronCrossSection)
                NeutronCrossSection -= Math.Min(0.1f, NeutronCrossSection - ConfiguredInsertionLevel);
            else
                NeutronCrossSection += Math.Min(0.1f, NeutronCrossSection - ConfiguredInsertionLevel);
        }

        return neutrons;
    }
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ReactorGasChannelComponent : ReactorPart
{
    [DataField]
    public float GasThermalCrossSection = 15;
    public GasMixture? AirContents;

    public GasMixture? ReturnAir() => AirContents;

    public override void Melt(IRobustRandom random)
    {
        base.Melt(random);
        GasThermalCrossSection = 0.1f;
    }

    public override List<ReactorNeutron> ProcessNeutrons(List<ReactorNeutron> neutrons, IRobustRandom random)
    {
        base.ProcessNeutrons(neutrons, random);
        // TODO: gas-neutron interactions
        return neutrons;
    }
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
        ThermalCrossSection = 10
    };

    public static readonly ReactorPartComponent FuelRod = new()
    {
        IconStateInserted = "fuel",
        IconStateCap = "fuel_cap",
        NeutronCrossSection = 1.0f,
        ThermalCrossSection = 10,
        ThermalMass = 420000,
        NRadioactive = 3,
        Radioactive = 4
    };

    public static readonly ReactorGasChannelComponent GasChannel = new()
    {
        IconStateInserted = "gas",
        IconStateCap = "gas_cap",
        ThermalCrossSection = 15,
        GasVolume = 100,
        ThermalMass = 21000
    };

    public static readonly ReactorPartComponent HeatExchanger = new()
    {
        IconStateInserted = "heat",
        IconStateCap = "heat_cap",
        NeutronCrossSection = 0.1f,
        ThermalCrossSection = 25
    };
}