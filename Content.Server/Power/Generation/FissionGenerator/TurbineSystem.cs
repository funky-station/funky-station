using Content.Server.Administration.Logs;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Piping.Components;
using Content.Shared.Audio;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Popups;
using Content.Shared.Power.Generation.FissionGenerator;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Explosion.Components;
using Content.Shared.Tools.Systems;
using Content.Shared.Interaction;
using Content.Shared.Repairable;
using Content.Shared.Damage;
using Content.Shared.Tools;
using Robust.Shared.Prototypes;

namespace Content.Server.Power.Generation.FissionGenerator;

public sealed class TurbineSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceDisabledEvent>(OnDisabled);
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceEnabledEvent>(OnEnabled);
        SubscribeLocalEvent<TurbineComponent, AtmosDeviceUpdateEvent>(OnUpdate);
        SubscribeLocalEvent<TurbineComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TurbineComponent, InteractUsingEvent>(RepairTurbine);
        SubscribeLocalEvent<TurbineComponent, RepairFinishedEvent>(OnRepairTurbineFinished);
    }

    private void OnEnabled(EntityUid uid, TurbineComponent comp, ref AtmosDeviceEnabledEvent args)
    {
        return;
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
        var supplier = Comp<PowerSupplierComponent>(uid);
        comp.SupplierMaxSupply = supplier.MaxSupply;

        supplier.MaxSupply = comp.LastGen;

        if (!_nodeContainer.TryGetNodes(uid, comp.InletName, comp.OutletName, out PipeNode? inlet, out PipeNode? outlet))
        {
            _ambientSoundSystem.SetAmbience(uid, false);
            comp.HasPipes = false;
            return;
        }
        else
        {
            comp.HasPipes = true;
        }

        _appearance.TryGetData<bool>(uid, TurbineVisuals.TurbineRuined, out var IsSpriteRuined);
        if (comp.Ruined)
        {
            
            if (!IsSpriteRuined)
            {
                _appearance.SetData(uid, TurbineVisuals.TurbineRuined, true);
            }
            _ambientSoundSystem.SetAmbience(uid, false);
            comp.RPM = 0;
        }
        else if(comp.RPM <= 0)
        {
            if (IsSpriteRuined)
            {
                _appearance.SetData(uid, TurbineVisuals.TurbineRuined, false);
            }
        }
        else if(comp.RPM>0)
        {
            switch (comp.RPM)
            {
                case float n when n is > 1 and <= 60:
                    _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, TurbineSpeed.SpeedSlow);
                    break;
                case float n when n > 60 && n <= comp.BestRPM * 0.5:
                    _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, TurbineSpeed.SpeedMid);
                    break;
                case float n when n > comp.BestRPM * 0.5 && n <= comp.BestRPM * 1.2:
                    _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, TurbineSpeed.SpeedFast);
                    break;
                case float n when n > comp.BestRPM * 1.2 && n <= float.PositiveInfinity:
                    _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, TurbineSpeed.SpeedOverspeed);
                    break;
                default:
                    _appearance.SetData(uid, TurbineVisuals.TurbineSpeed, TurbineSpeed.SpeedStill);
                    break;
            }
        }
        // TODO: change sprite based on RPM

        var InletStartingPressure = inlet.Air.Pressure;
        var TransferMoles = 0f;
        if (InletStartingPressure > 0)
        {
            TransferMoles = inlet.Air.Volume * InletStartingPressure / (Atmospherics.R * inlet.Air.Temperature);
        }
        var AirContents = inlet.Air.Remove(TransferMoles);

        comp.LastVolumeTransfer = AirContents.Volume;
        comp.LastGen = 0;
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

            if (!comp.Ruined && !_audio.IsPlaying(comp.AudioStreams[0]))
            {
                comp.AudioStreams[0] = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/alarm_buzzer.ogg"), uid, AudioParams.Default.WithLoop(true))?.Entity;
                _popupSystem.PopupEntity(Loc.GetString("turbine-overheat", ("owner", uid)), uid, PopupType.LargeCaution);
            }


            _atmosphereSystem.Merge(outlet.Air, AirContents!);

            // Prevent power from being gereated by residual gasses
            AirContents?.Clear();
        }
        else
        {
            comp.AudioStreams[0] = _audio.Stop(comp.AudioStreams[0]);
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
                if (!_audio.IsPlaying(comp.AudioStreams[1])) 
                {
                    comp.AudioStreams[1] = _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Machines/alarm_beep.ogg"), uid, AudioParams.Default.WithLoop(true).WithVolume(-4))?.Entity;
                }
                comp.Stalling = true;
                comp.RPM = 0;
            }
            else
            {
                comp.Stalling = false;
                comp.RPM = NextRPM;
            }

            if(comp.RPM>10)
            {
                if (_audio.IsPlaying(comp.AudioStreams[1])) { comp.AudioStreams[1] = _audio.Stop(comp.AudioStreams[1]); }
                // Sacrifices must be made to have a smooth ramp up:
                // This will generate 2 audio streams every second with up to 4 of them playing at once... surely this can't go wrong :clueless:
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_FarHorizons/Ambience/Objects/turbine_room.ogg"), uid, AudioParams.Default.WithPitchScale(comp.RPM / comp.BestRPM).WithVolume(-2));
            }

            // Calculate power generation
            comp.LastGen = comp.PowerMultiplier * comp.StatorLoad * (comp.RPM / 30) * (float)(1 / Math.Cosh(0.01 * (comp.RPM - comp.BestRPM)));

            if (float.IsNaN(comp.LastGen))
            {
                comp.LastGen = 0;
                return; // TODO: crash the game here
            }

            comp.Overspeed = comp.RPM > comp.BestRPM * 1.2;

            // Damage the turbines during overspeed, linear increase from 18% to 45% then stays at 45%
            var random = new Random();
            if (comp.Overspeed && random.NextFloat() < 0.15 * Math.Min(comp.RPM / comp.BestRPM, 3))
            {
                // TODO: damage flash
                _audio.PlayPvs(new SoundPathSpecifier(comp.DamageSoundList[random.Next(0, comp.DamageSoundList.Count-1)]), uid, AudioParams.Default.WithVariation(0.25f).WithVolume(-1));
                comp.BladeHealth--;
                UpdateHealthIndicators(uid, comp);
            }

            _atmosphereSystem.Merge(outlet.Air, AirContents);
        }
        inlet.Air.Volume = comp.FlowRate;
        AirContents!.Volume = comp.FlowRate;

        // Explode
        if (!comp.Ruined && comp.BladeHealth <= 0)
        {
            TearApart(uid, comp);
        }
    }

    private void OnDisabled(EntityUid uid, TurbineComponent comp, ref AtmosDeviceDisabledEvent args)
    {
        return;
    }

    private void UpdateHealthIndicators(EntityUid uid, TurbineComponent comp)
    {
        if (comp.BladeHealth <= 0.75 * comp.BladeHealthMax && !comp.IsSparking)
        {
            comp.IsSparking = true;
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/PowerSink/electric.ogg"), uid, AudioParams.Default.WithPitchScale(0.75f));
            _popupSystem.PopupEntity(Loc.GetString("turbine-spark", ("owner", uid)), uid, PopupType.MediumCaution);
        }
        else if (comp.BladeHealth > 0.75 * comp.BladeHealthMax && comp.IsSparking)
        {
            comp.IsSparking = false;
            _popupSystem.PopupEntity(Loc.GetString("turbine-spark-stop", ("owner", uid)), uid, PopupType.Medium);
        }

        if (comp.BladeHealth <= 0.5 * comp.BladeHealthMax && !comp.IsSmoking)
        {
            comp.IsSmoking = true;
            _popupSystem.PopupEntity(Loc.GetString("turbine-smoke", ("owner", uid)), uid, PopupType.MediumCaution);
        }
        else if (comp.BladeHealth > 0.5 * comp.BladeHealthMax && comp.IsSmoking)
        {
            comp.IsSmoking = false;
            _popupSystem.PopupEntity(Loc.GetString("turbine-smoke-stop", ("owner", uid)), uid, PopupType.Medium);
        }

        _appearance.SetData(uid, TurbineVisuals.DamageSpark, comp.IsSparking);
        _appearance.SetData(uid, TurbineVisuals.DamageSmoke, comp.IsSmoking);
    }

    private void TearApart(EntityUid uid, TurbineComponent comp)
    {
        _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/metal_break5.ogg"), uid, AudioParams.Default);
        _popupSystem.PopupEntity(Loc.GetString("turbine-explode", ("owner", uid)), uid, PopupType.LargeCaution);
        _explosion.TriggerExplosive(uid, Comp<ExplosiveComponent>(uid), false, comp.RPM / 10, 5);
        // TODO: shoot blades everywhere
        _adminLogger.Add(LogType.Explosion, LogImpact.Medium, $"{ToPrettyString(uid)} destroyed by overspeeding for too long");
        comp.Ruined = true;
    }

    private void RepairTurbine(Entity<TurbineComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        // Only try repair the target if it is damaged
        if (ent.Comp.BladeHealth >= ent.Comp.BladeHealthMax && !ent.Comp.Ruined)
            return;

        args.Handled = _toolSystem.UseTool(args.Used, args.User, ent.Owner, ent.Comp.RepairDelay, ent.Comp.RepairTool, new RepairFinishedEvent(), ent.Comp.RepairFuelCost);
    }

    private void OnRepairTurbineFinished(Entity<TurbineComponent> ent, ref RepairFinishedEvent args)
    {
        var MessageID = "";

        if (ent.Comp.Ruined)
        {
            MessageID = "turbine-repair-ruined";
            ent.Comp.Ruined = false;
            UpdateHealthIndicators(ent.Owner, ent.Comp);
        }
        else if(ent.Comp.BladeHealth <  ent.Comp.BladeHealthMax)
        {
            MessageID = "turbine-repair";
            ent.Comp.BladeHealth++;
            UpdateHealthIndicators(ent.Owner, ent.Comp);
        }
        else
        {
            MessageID = "turbine-no-damage";
        }

        _popupSystem.PopupClient(Loc.GetString(MessageID, ("target", ent.Owner), ("tool", args.Used!)), ent.Owner, args.User);
    }


    //private void UpdateAppearance(EntityUid uid, TurbineComponent? comp = null)
    //{
    //    return;
    //}
}
