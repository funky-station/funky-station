using System.Numerics;
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
    public override GasMixture? ProcessGas(ReactorPartComponent reactorPart, Entity<NuclearReactorComponent> reactorEnt, GasMixture inGas)
    {
        if (reactorPart.RodType != (byte)ReactorPartComponent.RodTypes.GasChannel)
            return null;

        GasMixture? ProcessedGas = null;
        if (reactorPart.AirContents != null)
        {
            var compTemp = reactorPart.Temperature;
            var gasTemp = reactorPart.AirContents.Temperature;

            var DeltaT = compTemp - gasTemp;
            var DeltaTr = (compTemp + gasTemp) * (compTemp - gasTemp) * (Math.Pow(compTemp, 2) + Math.Pow(gasTemp, 2));

            var k = (Math.Pow(10, reactorPart.PropertyThermal / 5) - 1) / 2;
            var A = reactorPart.GasThermalCrossSection * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(reactorPart.AirContents);

            var COECheck = ThermalEnergy + reactorPart.Temperature * reactorPart.ThermalMass;

            var Hottest = Math.Max(gasTemp, compTemp);
            var Coldest = Math.Min(gasTemp, compTemp);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (compTemp * reactorPart.ThermalMass) - (Hottest * reactorPart.ThermalMass),
                (compTemp * reactorPart.ThermalMass) - (Coldest * reactorPart.ThermalMass));

            reactorPart.AirContents.Temperature = (float)Math.Clamp(gasTemp +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(reactorPart.AirContents, true)), Coldest, Hottest);

            reactorPart.Temperature = (float)Math.Clamp(compTemp -
                ((_atmosphereSystem.GetThermalEnergy(reactorPart.AirContents) - ThermalEnergy) / reactorPart.ThermalMass), Coldest, Hottest);

            var COEVerify = _atmosphereSystem.GetThermalEnergy(reactorPart.AirContents) + reactorPart.Temperature * reactorPart.ThermalMass;
            if (Math.Abs(COEVerify - COECheck) > 64)
                throw new Exception("COE violation, difference of " + Math.Abs(COEVerify - COECheck));

            if (gasTemp < 0 || compTemp < 0)
                throw new Exception("Reactor part temperature went below 0k.");

            if (reactorPart.Melted)
            {
                var T = _atmosphereSystem.GetTileMixture(reactorEnt.Owner, excite: true);
                if (T != null)
                    _atmosphereSystem.Merge(T, reactorPart.AirContents);
            }
            else
                ProcessedGas = reactorPart.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            reactorPart.AirContents = inGas.RemoveVolume(reactorPart.GasVolume);
            reactorPart.AirContents.Volume = reactorPart.GasVolume;

            if (reactorPart.AirContents != null && reactorPart.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, reactorPart.AirContents);
                    reactorPart.AirContents.Clear();
                }
                else
                {
                    ProcessedGas = reactorPart.AirContents;
                    reactorPart.AirContents.Clear();
                }
            }
        }
        return ProcessedGas;
    }

    public override List<ReactorNeutron> ProcessNeutronsGas(ReactorPartComponent reactorPart, List<ReactorNeutron> neutrons) => neutrons;
}