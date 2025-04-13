// Assmos - /tg/ gases
using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Atmos.Piping.Unary.Components;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.Power.EntitySystems;
using Content.Shared.Atmos.Piping.Portable.Components;
using Content.Shared.Atmos.Visuals;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Content.Shared.Interaction;
using Content.Shared.Atmos;
using Content.Server.Atmos.Components;

namespace Content.Server.Atmos.Portable;

public sealed class ElectrolyzerSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly GasTileOverlaySystem _gasOverlaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ElectrolyzerComponent, AtmosDeviceUpdateEvent>(OnDeviceUpdated);
        SubscribeLocalEvent<ElectrolyzerComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<ElectrolyzerComponent, ActivateInWorldEvent>(OnActivate);
    }

    private void OnPowerChanged(EntityUid uid, ElectrolyzerComponent electrolyzer, ref PowerChangedEvent args)
    {
        UpdateAppearance(uid);
    }

    private void OnActivate(EntityUid uid, ElectrolyzerComponent electrolyzer, ActivateInWorldEvent args)
    {
        if (args.Handled)
            return;

        ApcPowerReceiverComponent? powerReceiver = null;
        if (!Resolve(uid, ref powerReceiver))
            return;

        _power.TogglePower(uid);

        UpdateAppearance(uid);
    }

    private void UpdateAppearance(EntityUid uid)
    {
        if (!_power.IsPowered(uid))
        {
            _appearance.SetData(uid, ElectrolyzerVisuals.State, ElectrolyzerState.Off);
            return;
        }
        else
        {
            _appearance.SetData(uid, ElectrolyzerVisuals.State, ElectrolyzerState.On);
        }
    }

    private void OnDeviceUpdated(EntityUid uid, ElectrolyzerComponent electrolyzer, ref AtmosDeviceUpdateEvent args)
    {
        if (!_power.IsPowered(uid))
        {
            return;
        }

        UpdateAppearance(uid);

        var mixture = _atmosphereSystem.GetContainingMixture(uid, args.Grid, args.Map);
        if (mixture is null) return;

        var initH2O = mixture.GetMoles(Gas.WaterVapor);
        var initHyperNob = mixture.GetMoles(Gas.HyperNoblium);
        var initBZ = mixture.GetMoles(Gas.BZ);
        var temperature = mixture.Temperature;

        if (initH2O > 0.05f) 
        {
            var porportion = Math.Min(initH2O * 0.5f, 2.5f);
            var efficiency = Math.Min(mixture.Temperature / 1123.15f * 0.75f, 0.75f);

            var h2oRemoved = porportion * 2f;
            var oxyProduced = porportion * efficiency;
            var hydrogenProduced = porportion * 2f * efficiency;

            mixture.AdjustMoles(Gas.WaterVapor, -h2oRemoved);
            mixture.AdjustMoles(Gas.Oxygen, oxyProduced);
            mixture.AdjustMoles(Gas.Hydrogen, hydrogenProduced);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            if (heatCap > Atmospherics.MinimumHeatCapacity)
                mixture.Temperature = Math.Max((mixture.Temperature * heatCap) / heatCap, Atmospherics.TCMB);
        } 

        if (initHyperNob > 0.01f && temperature < 150f)
        {
            var proportion = Math.Min(initHyperNob, 1.5f);
            mixture.AdjustMoles(Gas.HyperNoblium, -proportion);
            mixture.AdjustMoles(Gas.AntiNoblium, proportion * 0.5f);

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            if (heatCap > Atmospherics.MinimumHeatCapacity)
                mixture.Temperature = Math.Max((mixture.Temperature * heatCap) / heatCap, Atmospherics.TCMB);
        }

        if (initBZ > 0.01f)
        {
            var reactionEfficiency = Math.Min(initBZ * (1f - (float)Math.Pow(Math.E, -0.5f * temperature / Atmospherics.FireMinimumTemperatureToExist)), initBZ);
            mixture.AdjustMoles(Gas.BZ, -reactionEfficiency);
            mixture.AdjustMoles(Gas.Oxygen, reactionEfficiency * 0.2f);
            mixture.AdjustMoles(Gas.Halon, reactionEfficiency * 2f);
            var energyReleased = reactionEfficiency * Atmospherics.HalonProductionEnergy;

            var heatCap = _atmosphereSystem.GetHeatCapacity(mixture, true);
            if (heatCap > Atmospherics.MinimumHeatCapacity)
                mixture.Temperature = Math.Max((mixture.Temperature * heatCap + energyReleased) / heatCap, Atmospherics.TCMB);
        }

        _gasOverlaySystem.UpdateSessions();
    }
}