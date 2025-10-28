using Content.Server.Atmos.EntitySystems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos;
using DependencyAttribute = Robust.Shared.IoC.DependencyAttribute;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed class ReactorPartSystem : SharedReactorPartSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="reactorPart">The reactor part.</param>
    /// <param name="reactorEnt">The entity representing the reactor this part is inserted into.</param>
    /// <param name="inGas">The gas to be processed.</param>
    /// <returns></returns>
    public override GasMixture? ProcessGas(ReactorPart reactorPart, Entity<NuclearReactorComponent> reactorEnt, GasMixture inGas)
    {
        if (reactorPart is not ReactorGasChannelComponent comp)
            return null;

        GasMixture? ProcessedGas = null;
        if (comp.AirContents != null)
        {
            var compTemp = comp.Temperature;
            var gasTemp = comp.AirContents.Temperature;

            var DeltaT = compTemp - gasTemp;
            var DeltaTr = (compTemp + gasTemp) * (compTemp - gasTemp) * (Math.Pow(compTemp, 2) + Math.Pow(gasTemp, 2));

            var k = (Math.Pow(10, comp.PropertyThermal / 5) - 1) / 2;
            var A = comp.GasThermalCrossSection * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(comp.AirContents);

            var Hottest = Math.Max(gasTemp, compTemp);
            var Coldest = Math.Min(gasTemp, compTemp);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (compTemp * comp.ThermalMass) - (Hottest * comp.ThermalMass),
                (compTemp * comp.ThermalMass) - (Coldest * comp.ThermalMass));

            comp.AirContents.Temperature = (float)Math.Clamp(gasTemp +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(comp.AirContents, true)), Coldest, Hottest);

            comp.Temperature = (float)Math.Clamp(compTemp -
                ((_atmosphereSystem.GetThermalEnergy(comp.AirContents) - ThermalEnergy) / comp.ThermalMass), Coldest, Hottest);

            if (gasTemp < 0 || compTemp < 0)
                return inGas; // TODO: crash the game here

            if (comp.Melted)
            {
                var T = _atmosphereSystem.GetTileMixture(reactorEnt.Owner, excite: true);
                if (T != null)
                    _atmosphereSystem.Merge(T, comp.AirContents);
            }
            else
                ProcessedGas = comp.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            comp.AirContents = inGas.Remove(comp.GasVolume * inGas.Pressure / (Atmospherics.R * inGas.Temperature));
            comp.AirContents.Volume = comp.GasVolume;

            if (comp.AirContents != null && comp.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, comp.AirContents);
                    comp.AirContents.Clear();
                }
                else
                {
                    ProcessedGas = comp.AirContents;
                    comp.AirContents.Clear();
                }
            }
        }
        return ProcessedGas;
    }
}