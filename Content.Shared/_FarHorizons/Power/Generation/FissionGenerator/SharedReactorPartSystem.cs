using Content.Shared.Atmos;
using Content.Shared.Ghost;
using Robust.Shared.Random;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedReactorPartSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly float _rate = 5;
    private readonly float _bias = 1.5f;

    /// <summary>
    /// Melts the related ReactorPart.
    /// </summary>
    /// <param name="reactorPart">ReactorPart to be melted</param>
    public void Melt(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, SharedNuclearReactorSystem reactorSystem)
    {
        if (reactorPart.Melted)
            return;

        reactorPart.Melted = true;
        reactorPart.IconStateCap += "_melted_" + _random.Next(1, 4 + 1);
        reactorSystem.UpdateGridVisual(reactorEnt.Owner, reactorEnt.Comp);
        reactorPart.NeutronCrossSection = 5f;
        reactorPart.ThermalCrossSection = 20f;
        reactorPart.IsControlRod = false;

        if(reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
            reactorPart.GasThermalCrossSection = 0.1f;
    }

    public void ProcessHeat(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, List<ReactorPartComponent?> AdjacentComponents, SharedNuclearReactorSystem reactorSystem)
    {
        // Intercomponent calculation
        foreach (var RC in AdjacentComponents)
        {
            if (RC == null)
                continue;

            var DeltaT = reactorPart.Temperature - RC.Temperature;
            // The thermal conductivity for a steel/steel interaction in SS13. SS14 does not support material properties
            // like this, so this is the best I can do
            var k = (Math.Pow(10, reactorPart.PropertyThermal / 5) - 1 + (Math.Pow(10, RC.PropertyThermal / 5) - 1)) / 2;
            var A = Math.Min(reactorPart.ThermalCrossSection, RC.ThermalCrossSection);

            reactorPart.Temperature = (float)(reactorPart.Temperature - (k * A * (0.5 * 8) / reactorPart.ThermalMass * DeltaT));
            RC.Temperature = (float)(RC.Temperature - (k * A * (0.5 * 8) / RC.ThermalMass * -DeltaT));

            if (RC.Temperature < 0 || reactorPart.Temperature < 0)
                throw new Exception("Reactor part temperature went below 0k.");

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }

        // Component-Reactor calculation
        var reactor = reactorEnt.Comp;
        if (reactor != null)
        {
            var DeltaT = reactorPart.Temperature - reactor.Temperature;

            var k = (Math.Pow(10, reactorPart.PropertyThermal / 5) - 1 + (Math.Pow(10, 7 / 5) - 1)) / 2;
            var A = reactorPart.ThermalCrossSection;

            reactorPart.Temperature = (float)(reactorPart.Temperature - (k * A * (0.5 * 8) / reactorPart.ThermalMass * DeltaT));
            reactor.Temperature = (float)(reactor.Temperature - (k * A * (0.5 * 8) / reactor.ThermalMass * -DeltaT));

            if (reactor.Temperature < 0 || reactorPart.Temperature < 0)
                throw new Exception("Reactor/part temperature went below 0k.");

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }
        if (reactorPart.Temperature > reactorPart.MeltingPoint && reactorPart.MeltHealth > 0)
            reactorPart.MeltHealth -= _random.Next(10, 50 + 1);
        if (reactorPart.MeltHealth <= 0)
            Melt(reactorPart, reactorEnt, reactorSystem);
    }

    /// <summary>
    /// Returns a list of neutrons from the interation of the given ReactorPart and initial neutrons.
    /// </summary>
    /// <param name="reactorPart">Reactor part applying the calculations</param>
    /// <param name="neutrons">Neutrons to be processed</param>
    /// <returns></returns>
    public virtual List<ReactorNeutron> ProcessNeutrons(ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons, out float thermalEnergy)
    {
        thermalEnergy = 0;
        var flux = new List<ReactorNeutron>(neutrons);
        foreach(var neutron in flux)
        {
            if (Prob(reactorPart.PropertyDensity * _rate * reactorPart.NeutronCrossSection * _bias))
            {
                if (neutron.velocity <= 1 && Prob(_rate * reactorPart.NRadioactive * _bias)) // neutron stimulated emission
                {
                    reactorPart.NRadioactive -= 0.001f;
                    reactorPart.Radioactive += 0.0005f;
                    for (var i = 0; i < _random.Next(1, 5 + 1); i++)
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(2, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 50;
                    thermalEnergy += 50;
                }
                else if (neutron.velocity <= 5 && Prob(_rate * reactorPart.Radioactive * _bias)) // stimulated emission
                {
                    reactorPart.Radioactive -= 0.001f;
                    reactorPart.SpentFuel += 0.0005f;
                    for (var i = 0; i < _random.Next(1, 5 + 1); i++)
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 25;
                    thermalEnergy += 25;
                }
                else
                {
                    // Put control rods first so they'd have a bigger effect
                    if (reactorPart.IsControlRod)
                        neutron.velocity = 0;
                    else if (Prob(_rate * reactorPart.PropertyHard)) // reflection, based on hardness
                        // A really complicated way of saying do a 180 or a 180+/-45
                        neutron.dir = (neutron.dir.GetOpposite().ToAngle() + (_random.NextAngle() / 4) - (MathF.Tau / 8)).GetDir();
                    else
                        neutron.velocity--;

                    if (neutron.velocity <= 0)
                        neutrons.Remove(neutron);

                    reactorPart.Temperature += 1;
                    thermalEnergy += 1;
                }
            }
        }
        if (Prob(reactorPart.NRadioactive * _rate * reactorPart.NeutronCrossSection))
        {
            var count = _random.Next(1, 3 + 1);
            for (var i = 0; i < count; i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = 3 });
            }
            reactorPart.NRadioactive -= 0.001f;
            reactorPart.Radioactive += 0.0005f;
            reactorPart.Temperature += 5; thermalEnergy += 5;
        }
        if (Prob(reactorPart.Radioactive * _rate * reactorPart.NeutronCrossSection))
        {
            var count = _random.Next(1, 3 + 1); // Was 3+1
            for (var i = 0; i < count; i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
            }
            reactorPart.Radioactive -= 0.001f;
            reactorPart.SpentFuel += 0.0005f;
            reactorPart.Temperature += 1; thermalEnergy += 1;
        }

        if (reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.Control)
        {
            if (!reactorPart.Melted && (reactorPart.NeutronCrossSection != reactorPart.ConfiguredInsertionLevel))
            {
                if (reactorPart.ConfiguredInsertionLevel < reactorPart.NeutronCrossSection)
                    reactorPart.NeutronCrossSection -= Math.Min(0.1f, reactorPart.NeutronCrossSection - reactorPart.ConfiguredInsertionLevel);
                else
                    reactorPart.NeutronCrossSection += Math.Min(0.1f, reactorPart.ConfiguredInsertionLevel - reactorPart.NeutronCrossSection);
            }
        }

        if (reactorPart.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
            neutrons = ProcessNeutronsGas(reactorPart, neutrons);

        neutrons ??= [];
        return neutrons;
    }

    public virtual GasMixture? ProcessGas(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, GasMixture inGas) => null;

    public virtual List<ReactorNeutron> ProcessNeutronsGas(ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons) => neutrons;

    /// <summary>
    /// Returns true according to a percent chance
    /// </summary>
    /// <param name="chance">Double, 0-100 </param>
    /// <returns></returns>
    private bool Prob(double chance) => _random.NextDouble() <= chance / 100;
}