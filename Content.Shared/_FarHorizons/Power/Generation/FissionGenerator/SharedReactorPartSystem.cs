using Content.Shared.Atmos;
using Robust.Shared.Random;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

public abstract class SharedReactorPartSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly float _rate = 5;

    /// <summary>
    /// Melts the related ReactorPart.
    /// </summary>
    /// <param name="reactorPart">ReactorPart to be melted</param>
    public void Melt(ReactorPart reactorPart)
    {
        if (reactorPart.Melted)
            return;

        reactorPart.Melted = true;
        reactorPart.IconStateCap += "_melted_" + _random.Next(1, 4 + 1);
        // TODO: tell parent to update its looks
        reactorPart.NeutronCrossSection = 5f;
        reactorPart.ThermalCrossSection = 20f;
        reactorPart.IsControlRod = false;
    }

    public void Melt(ReactorGasChannelComponent gasChannel)
    {
        Melt(gasChannel as ReactorPart);
        gasChannel.GasThermalCrossSection = 0.1f;
    }

    public void ProcessHeat(ReactorPart reactorPart, Entity<NuclearReactorComponent> reactorEnt, List<ReactorPart?> AdjacentComponents)
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

            reactorPart.Temperature = (float)(reactorPart.Temperature - ((k * A * (0.5 * 8) / reactorPart.ThermalMass) * DeltaT));
            RC.Temperature = (float)(RC.Temperature - ((k * A * (0.5 * 8) / RC.ThermalMass) * -DeltaT));

            if (RC.Temperature < 0 || reactorPart.Temperature < 0)
                return; // TODO: crash the game here

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
                return; // TODO: crash the game here

            // This is where we'd put material-based temperature effects... IF WE HAD ANY
        }
        if (reactorPart.Temperature > reactorPart.MeltingPoint && reactorPart.MeltHealth > 0)
            reactorPart.MeltHealth -= _random.Next(10, 50 + 1);
        if (reactorPart.MeltHealth <= 0)
            Melt(reactorPart);
    }

    public List<ReactorNeutron> ProcessNeutrons(ReactorPart reactorPart, List<ReactorNeutron> neutrons)
    {
        // Why not use a foreach? It doesn't work. Don't ask why, it just doesn't.
        var flux = neutrons;
        for (var n = 0; n < flux.Count; n++)
        {
            var neutron = flux[n];

            if (Prob(reactorPart.PropertyDensity * _rate * reactorPart.NeutronCrossSection))
            {
                if (neutron.velocity <= 1 && Prob(_rate * reactorPart.NRadioactive)) // neutron stimulated emission
                {
                    reactorPart.NRadioactive -= 0.001f;
                    reactorPart.Radioactive += 0.0005f;
                    for (var i = 0; i < _random.Next(1, 5 + 1); i++)
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(2, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 50;
                }
                else if (neutron.velocity <= 5 && Prob(_rate * reactorPart.Radioactive)) // stimulated emission
                {
                    reactorPart.Radioactive -= 0.001f;
                    reactorPart.SpentFuel += 0.0005f;
                    for (var i = 0; i < _random.Next(1, 5 + 1); i++)
                    {
                        neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
                    }
                    neutrons.Remove(neutron);
                    reactorPart.Temperature += 25;
                }
                else
                {
                    if (Prob(_rate * reactorPart.PropertyHard)) // reflection, based on hardness
                        // A really complicated way of saying do a 180 or a 180+/-45
                        neutron.dir = (neutron.dir.GetOpposite().ToAngle() + (_random.NextAngle() / 4) - (MathF.Tau / 8)).GetDir();
                    else if (reactorPart.IsControlRod)
                        neutron.velocity = 0;
                    else
                        neutron.velocity--;

                    if (neutron.velocity <= 0)
                        neutrons.Remove(neutron);

                    reactorPart.Temperature += 1;
                }
            }
        }
        if (Prob(reactorPart.NRadioactive * _rate * reactorPart.NeutronCrossSection))
        {
            for (var i = 0; i < _random.Next(1, 3 + 1); i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = 3 });
            }
            reactorPart.NRadioactive -= 0.001f;
            reactorPart.Radioactive += 0.0005f;
            reactorPart.Temperature += 20;
        }
        if (Prob(reactorPart.Radioactive * _rate * reactorPart.NeutronCrossSection))
        {
            for (var i = 0; i < _random.Next(1, 3 + 1); i++)
            {
                neutrons.Add(new() { dir = _random.NextAngle().GetDir(), velocity = _random.Next(1, 3 + 1) });
            }
            reactorPart.Radioactive -= 0.001f;
            reactorPart.SpentFuel += 0.0005f;
            reactorPart.Temperature += 10;
        }

        neutrons ??= [];
        return neutrons;
    }

    public List<ReactorNeutron> ProcessNeutrons(ReactorControlRodComponent controlRod, List<ReactorNeutron> neutrons)
    {
        neutrons = ProcessNeutrons(controlRod as ReactorPart, neutrons);

        if (!controlRod.Melted && (controlRod.NeutronCrossSection != controlRod.ConfiguredInsertionLevel))
        {
            if (controlRod.ConfiguredInsertionLevel < controlRod.NeutronCrossSection)
                controlRod.NeutronCrossSection -= Math.Min(0.1f, controlRod.NeutronCrossSection - controlRod.ConfiguredInsertionLevel);
            else
                controlRod.NeutronCrossSection += Math.Min(0.1f, controlRod.ConfiguredInsertionLevel - controlRod.NeutronCrossSection);
        }

        return neutrons;
    }

    public List<ReactorNeutron> ProcessNeutrons(ReactorGasChannelComponent gasChannel, List<ReactorNeutron> neutrons)
    {
        ProcessNeutrons(gasChannel as ReactorPart, neutrons);
        // TODO: gas-neutron interactions
        return neutrons;
    }

    public virtual GasMixture? ProcessGas(ReactorPart reactorPart, Entity<NuclearReactorComponent> reactorEnt, GasMixture inGas) => null;

    /// <summary>
    /// Returns true according to a percent chance
    /// </summary>
    /// <param name="chance">Double, 0-100 </param>
    /// <returns></returns>
    private bool Prob(double chance) => _random.NextDouble() <= chance / 100;
}