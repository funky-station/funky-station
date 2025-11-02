using Content.Server._FarHorizons.NodeContainer.Nodes;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Atmos;
using Content.Shared.IdentityManagement;
using Content.Shared.Radiation.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using System.Linq;

namespace Content.Server._FarHorizons.Power.Generation.FissionGenerator;

public sealed class NuclearReactorSystem : SharedNuclearReactorSystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly NodeGroupSystem _nodeGroupSystem = default!;
    [Dependency] private readonly ReactorPartSystem _partSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = null!;

    private static readonly int _gridWidth = NuclearReactorComponent.ReactorGridWidth;
    private static readonly int _gridHeight = NuclearReactorComponent.ReactorGridHeight;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<NuclearReactorComponent, AtmosDeviceDisabledEvent>(OnDisabled);

        SubscribeLocalEvent<NuclearReactorComponent, EntInsertedIntoContainerMessage>(OnPartChanged);
        SubscribeLocalEvent<NuclearReactorComponent, EntRemovedFromContainerMessage>(OnPartChanged);
        SubscribeLocalEvent<NuclearReactorComponent, ReactorItemActionMessage>(OnItemActionMessage);
    }

    private void OnPartChanged(EntityUid uid, NuclearReactorComponent component, ContainerModifiedMessage args) => ReactorTryGetSlot(uid, "part_slot", out component.PartSlot!);

    private void OnEnabled(EntityUid uid, NuclearReactorComponent comp, ref AtmosDeviceEnabledEvent args)
    {
        comp.ComponentGrid = new ReactorPartComponent[_gridWidth, _gridHeight];
        var prefab = SelectPrefab(comp.Prefab);
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                comp.FluxGrid[x, y] = [];
                comp.ComponentGrid[x, y] = prefab[x, y] != null ? new ReactorPartComponent(prefab[x, y]!) : null;
            }
        }

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
        Array.Clear(comp.VisualGrid);
        Array.Clear(comp.FluxGrid);
        comp.AirContents?.Clear();
    }

    private void OnUpdate(Entity<NuclearReactorComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        _appearance.SetData(uid, ReactorVisuals.Sprite, comp.Melted ? Reactors.Melted : Reactors.Normal);

        ProcessCaseRadiation(ent);

        if (comp.Melted)
            return;

        if (comp.VisualGrid[0, 0].Id == 0)
        { InitGrid(ent); comp.ApplyPrefab = true; }

        if (comp.ApplyPrefab)
        {
            var prefab = SelectPrefab(comp.Prefab);
            for (var x = 0; x < _gridWidth; x++)
            {
                for (var y = 0; y < _gridHeight; y++)
                {
                    comp.ComponentGrid[x, y] = prefab[x, y] != null ? new ReactorPartComponent(prefab[x, y]!) : null;
                }
            }

            comp.ApplyPrefab = false;

            comp.ApplyPrefab = false;
            UpdateGridVisual(uid, comp);
        }

        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out OffsetPipeNode? inlet, out OffsetPipeNode? outlet))
            return;

        // Try to connect to a distant pipe
        // This is BAD and I HATE IT... and I'm too lazy to fix it
        if (inlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(inlet);
        if (outlet.ReachableNodes.Count == 0)
            _nodeGroupSystem.QueueReflood(outlet);

        _appearance.SetData(uid, ReactorVisuals.Input, inlet.Air.Moles.Sum() > 20);
        _appearance.SetData(uid, ReactorVisuals.Output, outlet.Air.Moles.Sum() > 20);

        var AirContents = new GasMixture();

        var TempRads = 0;

        var NeutronCount = 0;
        var MeltedComps = 0;
        var ControlRods = 0;
        var AvgControlRodInsertion = 0f;
        var TotalNRads = 0f;
        var TotalRads = 0f;
        var TotalSpent = 0f;
        var TempChange = 0f;

        var GasInput = inlet.Air.RemoveVolume(inlet.Air.Volume);

        AirContents.Volume = inlet.Air.Volume;
        GasInput.Volume = AirContents.Volume;

        // Even though it's probably bad for performace, we have to do the for x, for y loops 3 times
        // to ensure the processes do not interfere with each other

        // Rod interactions
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                if (comp.ComponentGrid![x, y] != null)
                {
                    var ReactorComp = comp.ComponentGrid[x, y];
                    var gas = _partSystem.ProcessGas(ReactorComp!, ent, GasInput);
                    GasInput.Volume -= ReactorComp!.GasVolume;

                    if (gas != null)
                        _atmosphereSystem.Merge(AirContents, gas);

                    _partSystem.ProcessHeat(ReactorComp, ent, GetGridNeighbors(comp, x, y), this);
                    comp.TemperatureGrid[x, y] = ReactorComp.Temperature;

                    if (ReactorComp.RodType == (byte)ReactorPartComponent.RodTypes.Control)
                    {
                        AvgControlRodInsertion += ReactorComp.NeutronCrossSection;
                        ReactorComp.ConfiguredInsertionLevel = comp.ControlRodInsertion;
                        ControlRods++;
                    }

                    if (ReactorComp.Melted)
                        MeltedComps++;

                    comp.FluxGrid[x, y] = _partSystem.ProcessNeutrons(ReactorComp, comp.FluxGrid[x, y], out var deltaT);
                    TempChange += deltaT;

                    TotalNRads += ReactorComp.NRadioactive;
                    TotalRads += ReactorComp.Radioactive;
                    TotalSpent += ReactorComp.SpentFuel;
                }
                else
                    comp.TemperatureGrid[x, y] = 0;
            }
        }

        // Snapshot of the flux grid that won't get messed up by the neutron calculations
        var flux = new List<ReactorNeutron>[_gridWidth, _gridHeight];
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                flux[x, y] = new List<ReactorNeutron>(comp.FluxGrid[x, y]);
                comp.NeutronGrid[x, y] = comp.FluxGrid[x, y].Count;
            }
        }

        // Move neutrons
        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                foreach (var neutron in flux[x,y])
                {
                    NeutronCount++;

                    var dir = neutron.dir.AsFlag();
                    // Bit abuse
                    var xmod = (((byte)dir >> 1) % 2) - (((byte)dir >> 3) % 2);
                    var ymod = (((byte)dir >> 2) % 2) - ((byte)dir % 2);

                    if (x + xmod >= 0 && y + ymod >= 0 && x + xmod <= _gridWidth - 1
                        && y + ymod <= _gridHeight - 1)
                    {
                        comp.FluxGrid[x + xmod, y + ymod].Add(neutron);
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

        var CasingGas = ProcessCasingGas(comp, GasInput);
        if (CasingGas != null)
            _atmosphereSystem.Merge(AirContents, CasingGas);

        // If there's still input gas left over
        _atmosphereSystem.Merge(AirContents, GasInput);

        // TODO: probably more for this
        if (comp.Temperature >= comp.ReactorOverheatTemp)
        {
            _appearance.SetData(uid, ReactorVisuals.Smoke, true);
            if (comp.Temperature >= comp.ReactorFireTemp)
            {
                _appearance.SetData(uid, ReactorVisuals.Fire, true);
            }
            else
            {
                _appearance.SetData(uid, ReactorVisuals.Fire, false);
            }
        }
        else
        {
            _appearance.SetData(uid, ReactorVisuals.Smoke, false);
        }

        comp.RadiationLevel = TempRads;
        comp.AccRadiation += TempRads*0.5f;

        comp.NeutronCount = NeutronCount;
        comp.MeltedParts = MeltedComps;
        comp.DetectedControlRods = ControlRods;
        comp.AvgInsertion = AvgControlRodInsertion / ControlRods;
        comp.TotalNRads = TotalNRads;
        comp.TotalRads = TotalRads;
        comp.TotalSpent = TotalSpent;

        for(var i = 1; i < comp.TempChange.Length; i++)
        {
            comp.TempChange[i-1]=comp.TempChange[i];
        }
        comp.TempChange[^1] = TempChange;
        comp.TempChangeAvg = comp.TempChange.Average();

        if (TempRads > 1000 || comp.Temperature > comp.ReactorMeltdownTemp)
        {
            CatastrophicOverload(ent);
        }

        _atmosphereSystem.Merge(outlet.Air, AirContents);

        UpdateVisuals(ent);
    }

    private void CatastrophicOverload(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;

        // TODO: Audio

        comp.Melted = true;
        var MeltdownBadness = 0f;
        comp.AirContents ??= new();

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                if(comp.ComponentGrid[x, y] != null)
                {
                    var RC = comp.ComponentGrid[x, y];
                    if (RC == null)
                        return;
                    MeltdownBadness += ((RC.Radioactive * 2) + (RC.NRadioactive * 5) + (RC.SpentFuel * 10)) * (RC.Melted ? 2 : 1);
                    if (RC.RodType == (byte)ReactorPartComponent.RodTypes.GasChannel)
                        _atmosphereSystem.Merge(comp.AirContents, RC.AirContents ?? new());
                }
            }
        }
        comp.AccRadiation += MeltdownBadness;
        comp.AirContents.AdjustMoles(Gas.Tritium, MeltdownBadness * 15);
        comp.AirContents.Temperature = Math.Max(comp.Temperature, comp.AirContents.Temperature);

        var T = _atmosphereSystem.GetTileMixture(ent.Owner, excite: true);
        if (T != null)
            _atmosphereSystem.Merge(T, comp.AirContents);

        // TODO: shrapnel
        // TODO: explosion

        // Reset grids
        Array.Clear(comp.ComponentGrid);
        Array.Clear(comp.NeutronGrid);
        Array.Clear(comp.TemperatureGrid);
        Array.Clear(comp.FluxGrid);

        UpdateGridVisual(ent.Owner, comp);
    }

    private static List<ReactorPartComponent?> GetGridNeighbors(NuclearReactorComponent reactor, int x, int y)
    {
        var neighbors = new List<ReactorPartComponent?>();
        if (x - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x - 1, y]);
        if (x + 1 >= _gridWidth)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x + 1, y]);
        if (y - 1 < 0)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y - 1]);
        if (y + 1 >= _gridHeight)
            neighbors.Add(null);
        else
            neighbors.Add(reactor.ComponentGrid[x, y + 1]);
        return neighbors;
    }

    private GasMixture? ProcessCasingGas(NuclearReactorComponent reactor, GasMixture inGas)
    {
        GasMixture? ProcessedGas = null;
        if (reactor.AirContents != null)
        {
            var DeltaT = reactor.Temperature - reactor.AirContents.Temperature;
            var DeltaTr = Math.Pow(reactor.Temperature, 4) - Math.Pow(reactor.AirContents.Temperature, 4);

            var k = (Math.Pow(10, 6 / 5) - 1) / 2;
            var A = 1 * (0.4 * 8);

            var ThermalEnergy = _atmosphereSystem.GetThermalEnergy(reactor.AirContents);

            var COECheck = ThermalEnergy + reactor.Temperature * reactor.ThermalMass;

            var Hottest = Math.Max(reactor.AirContents.Temperature, reactor.Temperature);
            var Coldest = Math.Min(reactor.AirContents.Temperature, reactor.Temperature);

            var MaxDeltaE = Math.Clamp((k * A * DeltaT) + (5.67037442e-8 * A * DeltaTr),
                (reactor.Temperature * reactor.ThermalMass) - (Hottest * reactor.ThermalMass),
                (reactor.Temperature * reactor.ThermalMass) - (Coldest * reactor.ThermalMass));

            reactor.AirContents.Temperature = (float)Math.Clamp(reactor.AirContents.Temperature +
                (MaxDeltaE / _atmosphereSystem.GetHeatCapacity(reactor.AirContents, true)), Coldest, Hottest);

            reactor.Temperature = (float)Math.Clamp(reactor.Temperature -
                ((_atmosphereSystem.GetThermalEnergy(reactor.AirContents) - ThermalEnergy) / reactor.ThermalMass), Coldest, Hottest);

            var COEVerify = _atmosphereSystem.GetThermalEnergy(reactor.AirContents) + reactor.Temperature * reactor.ThermalMass;
            if (Math.Abs(COEVerify - COECheck) > 64)
                //throw new Exception("COE violation, difference of " + Math.Abs(COEVerify - COECheck));

            if (reactor.AirContents.Temperature < 0 || reactor.Temperature < 0)
                throw new Exception("Reactor casing temperature calculation resulted in sub-zero value.");

            ProcessedGas = reactor.AirContents;
        }

        if (inGas != null && _atmosphereSystem.GetThermalEnergy(inGas) > 0)
        {
            reactor.AirContents = inGas.RemoveVolume(reactor.ReactorVesselGasVolume);

            if (reactor.AirContents != null && reactor.AirContents.TotalMoles < 1)
            {
                if (ProcessedGas != null)
                {
                    _atmosphereSystem.Merge(ProcessedGas, reactor.AirContents);
                    reactor.AirContents.Clear();
                }
                else
                {
                    ProcessedGas = reactor.AirContents;
                    reactor.AirContents.Clear();
                }
            }
        }
        return ProcessedGas;
    }

    private void ProcessCaseRadiation(Entity<NuclearReactorComponent> ent)
    {
        var reactor = ent.Comp;
        var comp = CompOrNull<RadiationSourceComponent>(ent.Owner);
        if (comp == null) return;

        comp.Intensity = (float)Math.Sqrt(Math.Max(reactor.AccRadiation, 0));
        reactor.AccRadiation -= Math.Min(comp.Intensity, reactor.AccRadiation);
    }

    private static ReactorPartComponent?[,] SelectPrefab(string select) => select switch
    {
        "normal" => NuclearReactorPrefabs.Normal,
        "debug" => NuclearReactorPrefabs.Debug,
        "meltdown" => NuclearReactorPrefabs.Meltdown,
        "alignment" => NuclearReactorPrefabs.Alignment,
        _ => NuclearReactorPrefabs.Empty,
    };

    private void InitGrid(Entity<NuclearReactorComponent> ent)
    {
        var xspace = 18f / 32f;
        var yspace = 15f / 32f;

        var yoff = 5f / 32f;

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                // ...48 entities stuck on the grid, spawn one more, pass it around, 49 entities stuck on the grid...
                ent.Comp.VisualGrid[x, y] = _entityManager.GetNetEntity(SpawnAttachedTo("ReactorComponent", new(ent.Owner, xspace * (y - 3), (-yspace * (x - 3)) - yoff)));
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

        var zoff = _gridWidth * _gridHeight;

        var temp = new double[_gridWidth * _gridHeight];
        var neutron = new int[_gridWidth * _gridHeight];
        var icon = new string[_gridWidth * _gridHeight];
        var partName = new string[_gridWidth * _gridHeight];
        var partInfo = new double[_gridWidth * _gridHeight * 3];

        for (var x = 0; x < _gridWidth; x++)
        {
            for (var y = 0; y < _gridHeight; y++)
            {
                var pos = (x * _gridWidth) + y;
                temp[pos] = reactor.TemperatureGrid[x, y];
                neutron[pos] = reactor.NeutronGrid[x, y];
                icon[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.IconStateInserted : "base";

                partName[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.Name : "empty";
                partInfo[pos] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.NRadioactive : 0;
                partInfo[pos + zoff] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.Radioactive : 0;
                partInfo[pos + zoff * 2] = reactor.ComponentGrid[x, y] != null ? reactor.ComponentGrid[x, y]!.SpentFuel : 0;
            }
        }

        // This is transmitting close to 2.3KB of data every tick... ouch
        _uiSystem.SetUiState(uid, NuclearReactorUiKey.Key,
           new NuclearReactorBuiState
           {
               TemperatureGrid = temp,
               NeutronGrid = neutron,
               IconGrid = icon,
               PartInfo = partInfo,
               PartName = partName,
               ItemName = reactor.PartSlot.Item != null ? Identity.Name((EntityUid)reactor.PartSlot.Item, _entityManager) : null,
           });
    }

    private void OnItemActionMessage(Entity<NuclearReactorComponent> ent, ref ReactorItemActionMessage args)
    {
        var comp = ent.Comp;
        var pos = args.Position;
        var part = comp.ComponentGrid[(int)pos.X, (int)pos.Y];

        if (comp.PartSlot.Item == null == (part == null))
            return;

        if (comp.PartSlot.Item == null)
        {
            if (part!.Melted) // No removing a part if it's melted
                return;

            var item = SpawnInContainerOrDrop("BaseReactorItem", ent.Owner, "part_slot");
            _metaDataSystem.SetEntityName(item, part!.Name);
            _entityManager.AddComponent(item, new ReactorPartComponent(part!));

            comp.ComponentGrid[(int)pos.X, (int)pos.Y] = null;
        }
        else
        {
            if (TryComp(comp.PartSlot.Item, out ReactorPartComponent? reactorPart))
                comp.ComponentGrid[(int)pos.X, (int)pos.Y] = new ReactorPartComponent(reactorPart);
            else
                return;

            comp.ComponentGrid[(int)pos.X, (int)pos.Y]!.Name = Identity.Name((EntityUid)comp.PartSlot.Item, _entityManager);
            _entityManager.DeleteEntity(comp.PartSlot.Item);
        }

        UpdateGridVisual(ent.Owner, comp);
    }

    private void UpdateVisuals(Entity<NuclearReactorComponent> ent)
    {
        var comp = ent.Comp;
        var uid = ent.Owner;

        // Temperature & radiation warning
        if (comp.Temperature >= comp.ReactorOverheatTemp || comp.RadiationLevel > 50)
            if (comp.Temperature >= comp.ReactorFireTemp || comp.RadiationLevel > 75)
                _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsMeltdown);
            else
                _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsWarning);
        else
            _appearance.SetData(uid, ReactorVisuals.Lights, ReactorWarningLights.LightsOff);

        // Status screen / side lights
        switch (comp.Temperature)
        {
            case float n when n is <= Atmospherics.T20C:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Off);
                break;
            case float n when n > Atmospherics.T20C && n <= comp.ReactorOverheatTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Active);
                break;
            case float n when n > comp.ReactorOverheatTemp && n <= comp.ReactorFireTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Overheat);
                break;
            case float n when n > comp.ReactorFireTemp && n <= comp.ReactorMeltdownTemp:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Meltdown);
                break;
            case float n when n > comp.ReactorMeltdownTemp && n <= float.PositiveInfinity:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Boom);
                break;
            default:
                _appearance.SetData(uid, ReactorVisuals.Status, ReactorStatusLights.Off);
                break;
        }
    }
}