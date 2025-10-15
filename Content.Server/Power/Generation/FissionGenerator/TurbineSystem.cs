using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.Power.Generation.FissionGenerator;

public sealed class TurbineSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceDisabledEvent>(OnDisabled);
        SubscribeLocalEvent<TurbineComponent, ExaminedEvent>(OnExamined);
    }

    private void OnEnabled(EntityUid uid, TurbineComponent comp, ref AtmosDeviceEnabledEvent args)
    {
        UpdateAppearance(uid, comp);
    }

    private void OnExamined(Entity<TurbineComponent> ent, ref ExaminedEvent args)
    {
        var comp = ent.Comp;
        if (!Comp<TransformComponent>(ent).Anchored || !args.IsInDetailsRange) // Not anchored? Out of range? No status.
            return;

        if (!_nodeContainer.TryGetNode(ent.Owner, comp.InletName, out PipeNode? inlet))
            return;

        using (args.PushGroup(nameof(TurbineComponent)))
        {
            if (!comp.Ruined)
            {
                switch (comp.RPM)
                {
                    case float n when n is >= 0 and <= 1:
                        args.PushMarkup(Loc.GetString("turbine-spinning-0")); // " The blades are not spinning."
                        break;
                    case float n when n is > 1 and <= 60:
                        args.PushMarkup(Loc.GetString("turbine-spinning-1")); // " The blades are turning slowly."
                        break;
                    case float n when n > 60 && n <= comp.BestRPM * 0.5:
                        args.PushMarkup(Loc.GetString("turbine-spinning-2")); // " The blades are spinning."
                        break;
                    case float n when n > comp.BestRPM * 0.5 && n <= comp.BestRPM * 1.2:
                        args.PushMarkup(Loc.GetString("turbine-spinning-3")); // " The blades are spinning quickly."
                        break;
                    case float n when n > comp.BestRPM * 1.2 && n <= float.PositiveInfinity:
                        args.PushMarkup(Loc.GetString("turbine-spinning-4")); // " The blades are spinning out of control!"
                        break;
                    default:
                        break;
                }
            }

            if (comp.Ruined)
            {
                args.PushMarkup(Loc.GetString("turbine-ruined")); // " <b>It's completely broken!</b>"
            }
            else if (comp.BladeHealth <= 0.25 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-3")); // " <b>It's critically damaged!</b>"
            }
            else if (comp.BladeHealth <= 0.5 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-2")); // " The turbine looks badly damaged!"
            }
            else if (comp.BladeHealth <= 0.75 * comp.BladeHealthMax)
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-1")); // " The turbine looks a bit scuffed!"
            }
            else
            {
                args.PushMarkup(Loc.GetString("turbine-damaged-0")); // " It appears to be in good condition."
            }
        }
    }

    private void OnUpdate(EntityUid uid, TurbineComponent comp, ref AtmosDeviceUpdateEvent args)
    {
        
        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out PipeNode? inlet, out PipeNode? outlet))
        {
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        // TODO: change sprite based on RPM

        var InletStartingPressure = inlet.Air.Pressure;
        var TransferMoles = 0f;
        if (InletStartingPressure > 0)
        {
            TransferMoles = inlet.Air.Volume * InletStartingPressure / (Atmospherics.R * inlet.Air.Temperature);
        }
        var AirContents = inlet.Air.Remove(TransferMoles);

        comp.Overtemp = AirContents?.Temperature >= comp.MaxTemp;
        comp.Undertemp = AirContents?.Temperature <= comp.MinTemp;

        // Dump gas into atmosphere
        if (comp.Ruined || comp.Overtemp)
        {
            var tile = _atmosphereSystem.GetTileMixture(uid, excite: true);

            if (tile != null)
            {
                _atmosphereSystem.Merge(tile, AirContents!);
            }

            // TODO: message and sound on overtemp (not ruined)

            _atmosphereSystem.Merge(outlet.Air, AirContents!);

            // Prevent power from being gereated by residual gasses
            AirContents?.Clear();
        }

        if (!comp.Ruined && AirContents != null)
        {
            var InputStartingEnergy = _atmosphereSystem.GetThermalEnergy(AirContents);
            var InputHeatCap = _atmosphereSystem.GetHeatCapacity(AirContents, true);

            // Prevents div by 0 if it would come up
            if (InputStartingEnergy <= 0)
            {
                InputStartingEnergy = 1;
            }
            if (InputHeatCap <= 0)
            {
                InputHeatCap = 1;
            }

            if (AirContents.Temperature > comp.MinTemp)
            {
                AirContents.Temperature = (float)Math.Max((InputStartingEnergy - ((InputStartingEnergy - (InputHeatCap * Atmospherics.T20C)) * 0.8)) / InputHeatCap, Atmospherics.T20C);
            }

            var OutputStartingEnergy = _atmosphereSystem.GetThermalEnergy(AirContents);
            var EnergyGenerated = comp.StatorLoad * (comp.RPM / 60);

            var DeltaE = InputStartingEnergy - OutputStartingEnergy;
            float NewRPM;

            if (DeltaE - EnergyGenerated > 0)
            {
                NewRPM = comp.RPM + (float)Math.Sqrt(2 * (Math.Max(DeltaE - EnergyGenerated, 0) / comp.TurbineMass));
            }
            else
            {
                NewRPM = comp.RPM - (float)Math.Sqrt(2 * (Math.Max(EnergyGenerated - DeltaE, 0) / comp.TurbineMass));
            }

            var NextGen = comp.StatorLoad * (Math.Max(NewRPM, 0) / 60);
            float NextRPM;

            if (DeltaE - NextGen > 0)
            {
                NextRPM = comp.RPM + (float)Math.Sqrt(2 * (Math.Max(DeltaE - NextGen, 0) / comp.TurbineMass));
            }
            else
            {
                NextRPM = comp.RPM - (float)Math.Sqrt(2 * (Math.Max(NextGen - DeltaE, 0) / comp.TurbineMass));
            }

            if (NewRPM < 0 || NextRPM < 0)
            {
                // Stator load is too high
                comp.Stalling = true;
                comp.RPM = 0;
                // TODO: sound
            }
            else
            {
                comp.Stalling = false;
                comp.RPM = NextRPM;
            }

            comp.LastGen = comp.PowerMultiplier * comp.StatorLoad * (comp.RPM / 30) * (float)(1 / Math.Cosh(0.01 * (comp.RPM - comp.BestRPM)));

            if (float.IsNaN(comp.LastGen))
            {
                return; // TODO: crash the game here
            }

            Comp<PowerSupplierComponent>(uid).MaxSupply = 10000;
            if (TryComp<PowerSupplierComponent>(uid, out var supplier))
            {
                supplier.MaxSupply = 10000;
            }

                comp.Overspeed = comp.RPM > comp.BestRPM * 1.2;
            var random = new Random();
            if (comp.Overspeed && random.NextFloat() > 0.15 * Math.Min(comp.RPM / comp.BestRPM, 3))
            {
                // TODO: Sound
                comp.BladeHealth--;
            }

            _atmosphereSystem.Merge(outlet.Air, AirContents);
        }
        inlet.Air.Volume = comp.FlowRate;
        AirContents!.Volume = comp.FlowRate;

        // Explode
        if (!comp.Ruined && comp.BladeHealth <= 0)
        {
            // TODO: Sound
            // TODO: visible message
            // TODO: shoot blades everywhere
            comp.Ruined = true;
            comp.RPM = 0;
        }
    }

    private void OnDisabled(EntityUid uid, TurbineComponent comp, ref AtmosDeviceDisabledEvent args)
    {
        return;
    }

    private void UpdateAppearance(EntityUid uid, TurbineComponent? comp = null)
    {
        return;
    }
}
