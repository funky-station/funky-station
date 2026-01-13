// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Funkystation.Atmos.Piping.Binary.Components;
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

        // UI messages
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

        var mode = comp.Inverted ? "≥" : "≤";
        var status = comp.Enabled ? "Enabled" : "Disabled";

        args.PushMarkup(Loc.GetString("temperature-gate-examined",
            ("status", status),
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
            _ui.OpenUi(uid, TemperatureGateUiKey.Key, actor.PlayerSession);

        args.Handled = true;
    }

    private void OnAtmosUpdate(EntityUid uid, TemperatureGateComponent comp, ref AtmosDeviceUpdateEvent args)
    {
        if (!comp.Enabled ||
            (TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered))
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

        comp.LastInputTemperature = inlet.Air.Temperature;

        bool shouldOpen = comp.Inverted
            ? inlet.Air.Temperature >= comp.Threshold
            : inlet.Air.Temperature <= comp.Threshold;

        if (!shouldOpen)
        {
            UpdateAppearance(uid, comp, isOpen: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var n1 = inlet.Air.TotalMoles;
        var n2 = outlet.Air.TotalMoles;
        var P1 = inlet.Air.Pressure;
        var P2 = outlet.Air.Pressure;

        if (P1 <= P2)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var T1 = inlet.Air.Temperature;
        var T2 = outlet.Air.Temperature;
        var V1 = inlet.Air.Volume;
        var V2 = outlet.Air.Volume;

        var denom = T1 * V2 + T2 * V1;
        if (denom <= 0f)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var transferMoles = n1 - (n1 + n2) * T2 * V1 / denom;

        if (transferMoles <= 0f)
        {
            UpdateAppearance(uid, comp, isOpen: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var removed = inlet.Air.Remove(transferMoles);
        _atmosphere.Merge(outlet.Air, removed);

        UpdateAppearance(uid, comp, isOpen: true, isFlowing: true);
        _ambientSoundSystem.SetAmbience(uid, removed.TotalMoles > 0.001f);
    }

    private void OnSetThresholdAndMode(EntityUid uid, TemperatureGateComponent comp, TemperatureGateSetThresholdAndModeMessage msg)
    {
        comp.Threshold = Math.Clamp(msg.Threshold, 2.7f, 12000f);
        comp.Inverted = msg.IsMinMode;

        DirtyUI(uid, comp);
    }

    private void OnToggleEnabled(EntityUid uid, TemperatureGateComponent comp, TemperatureGateToggleEnabledMessage msg)
    {
        comp.Enabled = msg.Enabled;
        UpdateAppearance(uid, comp);
        DirtyUI(uid, comp);
    }

    private void DirtyUI(EntityUid uid, TemperatureGateComponent comp)
    {
        var deviceName = Name(uid);

        _ui.SetUiState(uid, TemperatureGateUiKey.Key,
            new TemperatureGateBoundUserInterfaceState(
                deviceName,
                comp.Threshold,
                comp.Inverted,
                comp.Enabled));
    }

    private void UpdateAppearance(EntityUid uid, TemperatureGateComponent comp, bool isOpen = false, bool isFlowing = false)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        TemperatureGateState state;

        if (!comp.Enabled || TryComp<ApcPowerReceiverComponent>(uid, out var power) && !power.Powered)
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
