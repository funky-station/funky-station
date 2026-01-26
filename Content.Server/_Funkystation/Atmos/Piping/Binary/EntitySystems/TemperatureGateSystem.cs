using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Shared._Funkystation.Atmos.Piping.Binary.Components;
using Content.Shared.Audio;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Robust.Server.GameObjects;
using Robust.Shared.Player;

namespace Content.Server._Funkystation.Atmos.Piping.Binary.EntitySystems;

public sealed class TemperatureGateSystem : EntitySystem
{
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemperatureGateComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TemperatureGateComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TemperatureGateComponent, ActivateInWorldEvent>(OnActivate);
        SubscribeLocalEvent<TemperatureGateComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);

        // UI interaction messages
        SubscribeLocalEvent<TemperatureGateComponent, TemperatureGateSetThresholdAndModeMessage>(OnSetThresholdAndMode);
        SubscribeLocalEvent<TemperatureGateComponent, TemperatureGateToggleEnabledMessage>(OnToggleEnabled);
    }

    private void OnInit(EntityUid uid, TemperatureGateComponent comp, ComponentInit args)
    {
        UpdateAppearance(uid, comp);
    }

    private void OnExamined(EntityUid uid, TemperatureGateComponent comp, ExaminedEvent args)
    {
        if (!Transform(uid).Anchored || !args.IsInDetailsRange)
            return;

        var mode = comp.Inverted ? "temperature-gate-status-greater" : "temperature-gate-status-lesser";
        var statusLocId = comp.Enabled ? "temperature-gate-status-enabled" : "temperature-gate-status-disabled";

        args.PushMarkup(Loc.GetString("temperature-gate-examined",
            ("status", Loc.GetString(statusLocId)),
            ("mode", mode),
            ("threshold", comp.Threshold.ToString("F2"))));
    }

    private void OnActivate(EntityUid uid, TemperatureGateComponent comp, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (!Transform(uid).Anchored)
        {
            args.Handled = true;
            return;
        }

        if (TryComp<ActorComponent>(args.User, out var actor))
        {
            DirtyUI(uid, comp);
            _ui.OpenUi(uid, TemperatureGateUiKey.Key, actor.PlayerSession);
        }

        args.Handled = true;
    }

    private void OnAtmosUpdate(EntityUid uid, TemperatureGateComponent comp, ref AtmosDeviceUpdateEvent args)
    {
        // Device must be enabled and powered to do anything
        if (!comp.Enabled || (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered))
        {
            UpdateAppearance(uid, comp);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        if (!_nodeContainer.TryGetNodes<PipeNode, PipeNode>(
            uid, comp.InletName, comp.OutletName,
            out var inlet, out var outlet))
        {
            return;
        }

        // Check if temperature condition allows flow
        var shouldOpen = comp.Inverted
            ? inlet.Air.Temperature >= comp.Threshold
            : inlet.Air.Temperature <= comp.Threshold;

        if (!shouldOpen)
        {
            UpdateAppearance(uid, comp, isOpen: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var p1 = inlet.Air.Pressure;
        var p2 = outlet.Air.Pressure;

        // No flow if pressure is not pushing from inlet to outlet
        if (p1 <= p2)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }


        var n1 = inlet.Air.TotalMoles;
        var n2 = outlet.Air.TotalMoles;
        var t1 = inlet.Air.Temperature;
        var t2 = outlet.Air.Temperature;
        var v1 = inlet.Air.Volume;
        var v2 = outlet.Air.Volume;

        // Denominator weighting each side to achieve pressure equilibrium:
        // derived from PV=nT when solving (n1 - x) T1 / V1 = (n2 + x) T2 / V2, so it becomes T1 V2 + T2 V1
        var denom = t1 * v2 + t2 * v1;

        // Avoid division by zero or negative (invalid gas mix state)
        if (denom <= 0f)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        // This is the exact number of moles that would equalize pressure between the two sides
        // if transferred all at once, accounting for their different temperatures and volumes.
        var transferMoles = n1 - (n1 + n2) * t2 * v1 / denom;

        if (transferMoles <= 0f)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        // Remove transfer amount from inlet and merge into outlet
        var removed = inlet.Air.Remove(transferMoles);
        _atmosphere.Merge(outlet.Air, removed);

        UpdateAppearance(uid, comp, isOpen: true, isFlowing: true);
        _ambientSoundSystem.SetAmbience(uid, true);
    }

    private void OnSetThresholdAndMode(EntityUid uid, TemperatureGateComponent comp, TemperatureGateSetThresholdAndModeMessage msg)
    {
        comp.Threshold = Math.Clamp(msg.Threshold, comp.MinThreshold, comp.MaxThreshold);
        comp.Inverted = msg.IsMinMode;
        Dirty(uid, comp);
        DirtyUI(uid, comp);
        UpdateAppearance(uid, comp);
    }

    private void OnToggleEnabled(EntityUid uid, TemperatureGateComponent comp, TemperatureGateToggleEnabledMessage msg)
    {
        comp.Enabled = msg.Enabled;
        Dirty(uid, comp);
        DirtyUI(uid, comp);
        UpdateAppearance(uid, comp);
    }

    private void DirtyUI(EntityUid uid, TemperatureGateComponent comp)
    {
        var state = new TemperatureGateBoundUserInterfaceState(
            Name(uid),
            comp.Threshold,
            comp.Inverted,
            comp.Enabled);

        _ui.SetUiState(uid, TemperatureGateUiKey.Key, state);
    }

    private void UpdateAppearance(EntityUid uid, TemperatureGateComponent comp, bool isOpen = false, bool isFlowing = false)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        TemperatureGateState state;

        if (!comp.Enabled || (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered))
        {
            state = TemperatureGateState.Off;
        }
        else if (!isOpen || !isFlowing)
        {
            state = TemperatureGateState.On;
        }
        else
        {
            state = TemperatureGateState.Flow;
        }

        _appearance.SetData(uid, TemperatureGateVisuals.State, state, appearance);
    }
}
