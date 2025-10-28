using Content.Server._FarHorizons.NodeContainer.Nodes;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Radiation.Components;
using Robust.Server.GameObjects;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly NodeGroupSystem _nodeGroupSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;
    [Dependency] private readonly ReactorPartSystem _partSystem = default!;

    
    //public List<ReactorNeutron>[,] FluxGrid = new List<ReactorNeutron>[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];

    private GasMixture _currentGas = new();

    public double[,] TemperatureGrid = new double[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];
    public int[,] NeutronGrid = new int[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceDisabledEvent>(OnDisabled);
    }

    private void OnEnabled(EntityUid uid, NuclearReactorComponent comp, ref AtmosDeviceEnabledEvent args)
    {
        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                comp.FluxGrid[x, y] = [];
            }
        }

        comp.ComponentGrid = new ReactorPart[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];
        Array.Copy(SelectPrefab(comp.Prefab), comp.ComponentGrid, comp.ComponentGrid.Length);
        comp.ApplyPrefab = false;
        UpdateGridVisual(uid, comp);
    }

    private void OnDisabled(EntityUid uid, NuclearReactorComponent comp, ref AtmosDeviceDisabledEvent args)
    {
        comp.ApplyPrefab = default!;
        comp.Temperature = Atmospherics.T20C;

        foreach(var RC in comp.ComponentGrid)
            if(RC != null)
                RC.Temperature = Atmospherics.T20C;

        Array.Clear(comp.ComponentGrid);
        Array.Clear(_reactorGrid);
        Array.Clear(comp.FluxGrid);

        _currentGas.Clear();
    }

    private void OnUpdate(Entity<NuclearReactorComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        if (comp.Melted)
            return;

        if (_reactorGrid[0, 0].Id == 0)
        { InitGrid(uid); comp.ApplyPrefab = true; }

        if (comp.ApplyPrefab)
        {
            Array.Copy(SelectPrefab(comp.Prefab), comp.ComponentGrid, comp.ComponentGrid.Length);
            comp.ApplyPrefab = false;
            UpdateGridVisual(uid, comp);
        }

        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out OffsetPipeNode? inlet, out OffsetPipeNode? outlet))
            return;

        // Try to connect to a distant pipe
        if (inlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(inlet);
        if (outlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(outlet);

        var AirContents = new GasMixture();

        var InletStartingPressure = inlet.Air.Pressure;
        var TempRads = 0;

        var NeutronCount = 0;
        var MeltedComps = 0;
        var ControlRods = 0;
        var AvgControlRodInsertion = 0f;
        var TotalNRads = 0f;
        var TotalRads = 0f;
        var TotalSpent = 0f;

        var TransferMoles = 0f;
        if (InletStartingPressure > 0)
        {
            TransferMoles = inlet.Air.Volume * InletStartingPressure / (Atmospherics.R * inlet.Air.Temperature);
        }
        var GasInput = inlet.Air.Remove(TransferMoles);

        AirContents.Volume = inlet.Air.Volume;
        GasInput.Volume = AirContents.Volume;

        // Snapshot of the flux grid that won't get messed up during the neutron calculations
        var flux = new List<ReactorNeutron>[NuclearReactorComponent.ReactorGridWidth, NuclearReactorComponent.ReactorGridHeight];
        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                if (flux[x, y] == null)
                    flux[x, y] = [];
                if (comp.ComponentGrid![x, y] != null)
                {
                    var ReactorComp = comp.ComponentGrid[x, y];
                    var gas = _partSystem.ProcessGas(ReactorComp!, ent, GasInput);
                    GasInput.Volume -= ReactorComp!.GasVolume;

                    if (gas != null)
                        _atmosphereSystem.Merge(AirContents, gas);

                    _partSystem.ProcessHeat(ReactorComp, ent, GetGridNeighbors(comp, x, y));
                    TemperatureGrid[x, y] = ReactorComp.Temperature;

                    if (ReactorComp is ReactorControlRodComponent ControlRod)
                    {
                        AvgControlRodInsertion += ControlRod.NeutronCrossSection;
                        ControlRod.ConfiguredInsertionLevel = comp.ControlRodInsertion;
                        ControlRods++;
                    }

                    if (ReactorComp.Melted)
                        MeltedComps++;

                    comp.FluxGrid[x, y] = _partSystem.ProcessNeutronsSwitch(ReactorComp, comp.FluxGrid[x, y]);

                    TotalNRads += ReactorComp.NRadioactive;
                    TotalRads += ReactorComp.Radioactive;
                    TotalSpent += ReactorComp.SpentFuel;
                }
                else
                    TemperatureGrid[x, y] = 0;

                NeutronGrid[x, y] = comp.FluxGrid[x, y].Count;
                
                for (var i = 0; i < comp.FluxGrid[x, y].Count; i++)
                {
                    var neutron = comp.FluxGrid[x, y][i];
                    NeutronCount++;

                    var dir = neutron.dir.AsFlag();
                    // Bit abuse
                    var xmod = (((byte)dir >> 1) % 2) - (((byte)dir >> 3) % 2);
                    var ymod = (((byte)dir >> 2) % 2) - ((byte)dir % 2);

                    if (x + xmod >= 0 && y + ymod >= 0 && x + xmod <= NuclearReactorComponent.ReactorGridWidth - 1
                        && y + ymod <= NuclearReactorComponent.ReactorGridHeight - 1)
                    {
                        if (flux[x + xmod, y + ymod] == null) // This is lazy and bad
                            flux[x + xmod, y + ymod] = [];
                        flux[x + xmod, y + ymod].Add(neutron);
                        comp.FluxGrid[x, y].Remove(neutron);
                    }
                    else
                    {
                        comp.FluxGrid[x, y].Remove(neutron);
                        TempRads++; // neutrons hitting the casing get blasted in to the room - have fun with that engineers!
                    }
                }
            }
        }
        Array.Copy(flux, comp.FluxGrid, comp.FluxGrid.Length);

        var CasingGas = ProcessCasingGas(comp, GasInput);
        if (CasingGas != null)
            _atmosphereSystem.Merge(AirContents, CasingGas);

        // If there's still input gas left over
        _atmosphereSystem.Merge(AirContents, GasInput);

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
        comp.NeutronCount = NeutronCount;
        comp.MeltedParts = MeltedComps;
        comp.DetectedControlRods = ControlRods;
        comp.AvgInsertion = AvgControlRodInsertion / ControlRods;
        comp.TotalNRads = TotalNRads;
        comp.TotalRads = TotalRads;
        comp.TotalSpent = TotalSpent;

        if (TempRads > 1000 || comp.Temperature > comp.ReactorMeltdownTemp)
        {
            // Explode
            return;
        }

        ProcessCaseRadiation(uid, TempRads);

        _atmosphereSystem.Merge(outlet.Air, AirContents);
    }

    private static List<ReactorPart?> GetGridNeighbors(NuclearReactorComponent reactor, int x, int y)
    {
        var neighbors = new List<ReactorPart?>();
        if (x - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x - 1, y]);
        if (x + 1 >= NuclearReactorComponent.ReactorGridWidth)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x + 1, y]);
        if (y - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y - 1]);
        if (y + 1 >= NuclearReactorComponent.ReactorGridHeight)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y + 1]);
        return neighbors;
    }

    private GasMixture? ProcessCasingGas(NuclearReactorComponent reactor, GasMixture inGas)
    {
        GasMixture? ProcessedGas = null;
        if (_currentGas != null)
        {
            var DeltaT = reactor.Temperature - _currentGas.Temperature;
            var DeltaTr = Math.Pow(reactor.Temperature, 4) - Math.Pow(_currentGas.Temperature, 4);

            var k = (Math.Pow(10, 6 / 5) - 1) / 2;
            var A = 1 * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(_currentGas);

            var Hottest = Math.Max(_currentGas.Temperature, reactor.Temperature);
            var Coldest = Math.Min(_currentGas.Temperature, reactor.Temperature);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (reactor.Temperature * reactor.ThermalMass) - (Hottest * reactor.ThermalMass),
                (reactor.Temperature * reactor.ThermalMass) - (Coldest * reactor.ThermalMass));

            _currentGas.Temperature = (float)Math.Clamp(_currentGas.Temperature +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(_currentGas, true)), Coldest, Hottest);

            reactor.Temperature = (float)Math.Clamp(reactor.Temperature -
                ((_atmosphereSystem.GetThermalEnergy(_currentGas) - ThermalEnergy) / reactor.ThermalMass), Coldest, Hottest);

            if (_currentGas.Temperature < 0 || reactor.Temperature < 0)
                return inGas; // TODO: crash the game here

            ProcessedGas = _currentGas;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            _currentGas = inGas.Remove(reactor.ReactorVesselGasVolume * inGas.Pressure / (Atmospherics.R * inGas.Temperature));

            if (_currentGas != null && _currentGas.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, _currentGas);
                    _currentGas.Clear();
                }
                else
                {
                    ProcessedGas = _currentGas;
                    _currentGas.Clear();
                }
            }
        }
        return ProcessedGas;
    }

    private void ProcessCaseRadiation(EntityUid uid, float rads)
    {
        var comp = CompOrNull<RadiationSourceComponent>(uid);
        if (comp == null) return;

        // Slow ramp up to 25 emitted rads at 1000 rads 
        //comp.Intensity = 24 * (float)Math.Log10((rads / 100) + 1);
        comp.Intensity = (comp.Intensity + rads) * 0.8f;
    }

    private static ReactorPart?[,] SelectPrefab(string select) => select switch
    {
        "normal" => NuclearReactorPrefabs.Normal,
        "debug" => NuclearReactorPrefabs.Debug,
        "meltdown" => NuclearReactorPrefabs.Meltdown,
        "alignment" => NuclearReactorPrefabs.Alignment,
        _ => NuclearReactorPrefabs.Empty,
    };

    private void InitGrid(EntityUid reactor)
    {
        var xspace = 18f / 32f;
        var yspace = 15f / 32f;

        var yoff = 6f / 32f;

        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                // ...48 entities stuck on the grid, spawn one more, pass it around, 49 entities stuck on the grid...
                _reactorGrid[x, y] = SpawnAttachedTo("ReactorComponent", new(reactor, xspace * (y - 3), (-yspace * (x - 3)) - yoff));
            }
        }
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<NuclearReactorComponent>();

        while (query.MoveNext(out var uid, out var reactor))
        {
            UpdateUI(uid, reactor);
        }
    }

    private void UpdateUI(EntityUid uid, NuclearReactorComponent reactor)
    {
        if (!_uiSystem.IsUiOpen(uid, NuclearReactorUiKey.Key))
            return;

        var temp = new double[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight];
        var neutron = new int[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight];

        for (var x = 0; x < NuclearReactorComponent.ReactorGridWidth; x++)
        {
            for (var y = 0; y < NuclearReactorComponent.ReactorGridHeight; y++)
            {
                temp[x * NuclearReactorComponent.ReactorGridWidth + y] = TemperatureGrid[x, y];
                neutron[x * NuclearReactorComponent.ReactorGridWidth + y] = NeutronGrid[x, y];
            }
        }

        _uiSystem.SetUiState(uid, NuclearReactorUiKey.Key,
           new NuclearReactorBuiState
           {
               TemperatureGrid = temp,
               NeutronGrid = neutron,
           });
    }
}