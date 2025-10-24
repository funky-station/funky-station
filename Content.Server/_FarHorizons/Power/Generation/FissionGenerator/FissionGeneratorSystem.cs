using Content.Server._FarHorizons.NodeContainer.Nodes;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Ghost;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos;
using Content.Shared.Radiation.Components;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed class FissionGeneratorSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    // Woe, 3 dimentions be upon ye
    public List<ReactorNeutron>[,] FluxGrid = new List<ReactorNeutron>[FissionGeneratorComponent.ReactorGridWidth, FissionGeneratorComponent.ReactorGridHeight];

    private GasMixture _airContents = new();
    private GasMixture? _currentGas;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FissionGeneratorComponent, AtmosDeviceUpdateEvent>(OnUpdate);

        for (var x = 0; x < FissionGeneratorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < FissionGeneratorComponent.ReactorGridHeight; y++)
            {
                FluxGrid[x, y] = new List<ReactorNeutron>();
            }
        }
    }

    private void OnUpdate(EntityUid uid, FissionGeneratorComponent comp, ref AtmosDeviceUpdateEvent args)
    {
        if (comp.Melted)
            return;
        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out OffsetPipeNode? inlet, out OffsetPipeNode? outlet))
            return;

        // Process last tick's air
        _atmosphereSystem.Merge(outlet.Air, _airContents);
        _airContents = new();

        var InletStartingPressure = inlet.Air.Pressure;
        var TempRads = 0;

        var TransferMoles = 0f;
        if (InletStartingPressure > 0)
        {
            TransferMoles = inlet.Air.Volume * InletStartingPressure / (Atmospherics.R * inlet.Air.Temperature);
        }
        var GasInput = inlet.Air.Remove(TransferMoles);

        _airContents.Volume = inlet.Air.Volume;
        GasInput.Volume = _airContents.Volume;

        for (var x = 0; x < FissionGeneratorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < FissionGeneratorComponent.ReactorGridHeight; y++)
            {
                if (comp.ComponentGrid[x, y] != null)
                {
                    comp.ComponentGrid[x, y].AssignParentReactor(comp);

                    var ReactorComp = comp.ComponentGrid[x, y];
                    var gas = ProcessGas(uid, ReactorComp, GasInput);
                    GasInput.Volume -= ReactorComp.GasVolume;

                    if (gas != null)
                        _atmosphereSystem.Merge(_airContents, gas);

                    ReactorComp.ProcessHeat(GetGridNeighbors(comp, x, y));

                    FluxGrid[x, y] = ReactorComp.ProcessNeutrons(FluxGrid[x, y]);
                }

                foreach (var neutron in FluxGrid[x, y])
                {
                    var dir = neutron.dir.AsFlag();
                    // Bit abuse
                    var xmod = (((byte)dir >> 1) % 2) - (((byte)dir >> 3) % 2);
                    var ymod = (((byte)dir >> 2) % 2) - ((byte)dir % 2);

                    if (x + xmod >= 0 && y + ymod >= 0 && x + xmod <= FissionGeneratorComponent.ReactorGridWidth
                        && y + ymod <= FissionGeneratorComponent.ReactorGridHeight)
                    {
                        FluxGrid[x + xmod, y + ymod].Add(neutron);
                        FluxGrid[x, y].Remove(neutron);
                    }
                    else
                    {
                        FluxGrid[x, y].Remove(neutron);
                        TempRads++; // neutrons hitting the casing get blasted in to the room - have fun with that engineers!
                    }
                }
            }
        }

        var CasingGas = ProcessCasingGas(comp, GasInput);
        if (CasingGas != null)
            _atmosphereSystem.Merge(_airContents, CasingGas);

        // If there's still input gas left over
        _atmosphereSystem.Merge(_airContents, GasInput);

        if (comp.Temperature >= comp.ReactorOverheatTemp)
        {
            // Overheat smoke
            if (comp.Temperature >= comp.ReactorFireTemp)
            {
                // Fire
            }
            else
            {
                // Stop Fire
            }
        }
        else
        {
            // Stop smoke
        }

        comp.RadiationLevel = TempRads;
        if (TempRads > 1000 || comp.Temperature > comp.ReactorMeltdownTemp)
        {
            // Explode
            return;
        }

        ProcessCaseRadiation(uid, TempRads);
    }

    private static List<ReactorPart?> GetGridNeighbors(FissionGeneratorComponent reactor, int x, int y)
    {
        var neighbors = new List<ReactorPart?>();
        if (x - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x - 1, y]);
        if (x + 1 >= FissionGeneratorComponent.ReactorGridWidth)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x + 1, y]);
        if (y - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y - 1]);
        if (y + 1 >= FissionGeneratorComponent.ReactorGridHeight)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y + 1]);
        return neighbors;
    }

    private GasMixture? ProcessGas(EntityUid uid, ReactorPart reactorComp, GasMixture inGas)
    {
        if (reactorComp is not ReactorGasChannelComponent gasChannel)
            return null;

        GasMixture? ProcessedGas = null;
        if (gasChannel.AirContents != null)
        {
            var DeltaT = gasChannel.Temperature - gasChannel.AirContents.Temperature;
            var DeltaTr = Math.Pow(gasChannel.Temperature, 4) - Math.Pow(gasChannel.AirContents.Temperature, 4);

            var k = (Math.Pow(10, gasChannel._propertyThermal / 5) - 1) / 2;
            var A = gasChannel.GasThermalCrossSection * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(gasChannel.AirContents);

            var Hottest = Math.Max(gasChannel.AirContents.Temperature, gasChannel.Temperature);
            var Coldest = Math.Min(gasChannel.AirContents.Temperature, gasChannel.Temperature);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (gasChannel.Temperature * gasChannel.ThermalMass) - (Hottest * gasChannel.ThermalMass),
                (gasChannel.Temperature * gasChannel.ThermalMass) - (Coldest * gasChannel.ThermalMass));

            gasChannel.AirContents.Temperature = (float)Math.Clamp(gasChannel.AirContents.Temperature +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(gasChannel.AirContents, true)), Coldest, Hottest);

            gasChannel.Temperature = (float)Math.Clamp(gasChannel.Temperature -
                ((_atmosphereSystem.GetThermalEnergy(gasChannel.AirContents) - ThermalEnergy) / gasChannel.ThermalMass), Coldest, Hottest);

            if (gasChannel.AirContents.Temperature < 0 || gasChannel.Temperature < 0)
                return inGas; // TODO: crash the game here

            if (gasChannel.Melted)
            {
                var T = _atmosphereSystem.GetTileMixture(uid, excite: true);
                if (T != null)
                    _atmosphereSystem.Merge(T, gasChannel.AirContents);
            }
            else
                ProcessedGas = gasChannel.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            gasChannel.AirContents = inGas.Remove(gasChannel.GasVolume * inGas.Pressure / (Atmospherics.R * inGas.Temperature));
            gasChannel.AirContents.Volume = gasChannel.GasVolume;

            if (gasChannel.AirContents != null && gasChannel.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, gasChannel.AirContents);
                    gasChannel.AirContents = null;
                }
                else
                {
                    ProcessedGas = gasChannel.AirContents;
                    gasChannel.AirContents = null;
                }
            }
        }
        return ProcessedGas;
    }

    private GasMixture? ProcessCasingGas(FissionGeneratorComponent gasChannel, GasMixture inGas)
    {
        GasMixture? ProcessedGas = null;
        if (_currentGas != null)
        {
            var DeltaT = gasChannel.Temperature - _currentGas.Temperature;
            var DeltaTr = Math.Pow(gasChannel.Temperature, 4) - Math.Pow(_currentGas.Temperature, 4);

            var k = (Math.Pow(10, 6 / 5) - 1) / 2;
            var A = 1 * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(_currentGas);

            var Hottest = Math.Max(_currentGas.Temperature, gasChannel.Temperature);
            var Coldest = Math.Min(_currentGas.Temperature, gasChannel.Temperature);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (gasChannel.Temperature * gasChannel.ThermalMass) - (Hottest * gasChannel.ThermalMass),
                (gasChannel.Temperature * gasChannel.ThermalMass) - (Coldest * gasChannel.ThermalMass));

            _currentGas.Temperature = (float)Math.Clamp(_currentGas.Temperature +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(_currentGas, true)), Coldest, Hottest);

            gasChannel.Temperature = (float)Math.Clamp(gasChannel.Temperature -
                ((_atmosphereSystem.GetThermalEnergy(_currentGas) - ThermalEnergy) / gasChannel.ThermalMass), Coldest, Hottest);

            if (_currentGas.Temperature < 0 || gasChannel.Temperature < 0)
                return inGas; // TODO: crash the game here


            ProcessedGas = _currentGas;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            _currentGas = inGas.Remove(gasChannel.ReactorVesselGasVolume * inGas.Pressure / (Atmospherics.R * inGas.Temperature));

            if (_currentGas != null && _currentGas.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, _currentGas);
                    _currentGas = null;
                }
                else
                {
                    ProcessedGas = _currentGas;
                    _currentGas = null;
                }
            }
        }
        return ProcessedGas;
    }

    private void ProcessCaseRadiation(EntityUid uid, float rads)
    {
        if (rads <= 0) return;

        var comp = Comp<RadiationSourceComponent>(uid);
        if (comp == null) return;

        // Slow ramp up to 25 emitted rads at 1000 rads 
        comp.Intensity = 24 * (float)Math.Log10((rads / 100) + 1);
    }
}