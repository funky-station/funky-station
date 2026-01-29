// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Atmos.Piping.Binary.Components;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Power.EntitySystems;
using Robust.Shared.Player;

namespace Content.Shared._Funkystation.Atmos.Piping.Binary.EntitySystems;

public abstract class SharedTemperatureGateSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TemperatureGateComponent, ComponentInit>(OnComponentInit);

        // Examine and activate events
        SubscribeLocalEvent<TemperatureGateComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<TemperatureGateComponent, ActivateInWorldEvent>(OnActivate);

        // UI interaction messages
        SubscribeLocalEvent<TemperatureGateComponent, TemperatureGateSetThresholdAndModeMessage>(OnSetThresholdAndMode);
        SubscribeLocalEvent<TemperatureGateComponent, TemperatureGateToggleEnabledMessage>(OnToggleEnabled);
    }

    protected virtual void OnComponentInit(EntityUid uid, TemperatureGateComponent comp, ComponentInit args)
    {
        UpdateAppearance(uid, comp);
    }

    protected virtual void OnExamined(EntityUid uid, TemperatureGateComponent comp, ExaminedEvent args)
    {
        if (!Transform(uid).Anchored || !args.IsInDetailsRange)
            return;

        var mode = comp.Inverted ? "temperature-gate-status-greater" : "temperature-gate-status-lesser";
        var statusLocId = comp.Enabled ? "temperature-gate-status-enabled" : "temperature-gate-status-disabled";

        args.PushMarkup(Loc.GetString("temperature-gate-examined",
            ("status", Loc.GetString(statusLocId)),
            ("mode", Loc.GetString(mode)),
            ("threshold", comp.Threshold.ToString("F2"))));
    }

    protected virtual void OnActivate(EntityUid uid, TemperatureGateComponent comp, ActivateInWorldEvent args)
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
            var state = new TemperatureGateBoundUserInterfaceState(
                Name(uid),
                comp.Threshold,
                comp.Inverted,
                comp.Enabled);

            _ui.SetUiState(uid, TemperatureGateUiKey.Key, state);
            _ui.OpenUi(uid, TemperatureGateUiKey.Key, actor.PlayerSession);
        }

        args.Handled = true;
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

    protected void UpdateAppearance(EntityUid uid, TemperatureGateComponent comp, bool isOn = false, bool isFlowing = false)
    {
        if (!TryComp<AppearanceComponent>(uid, out var appearance))
            return;

        TemperatureGateState state;

        if (!comp.Enabled || !_power.IsPowered((uid, null)))
        {
            state = TemperatureGateState.Off;
        }
        else if (!isOn || !isFlowing)
        {
            state = TemperatureGateState.On;
        }
        else
        {
            state = TemperatureGateState.Flow;
        }

        _appearance.SetData(uid, TemperatureGateVisuals.State, state, appearance);
    }

    protected void DirtyUI(EntityUid uid, TemperatureGateComponent comp)
    {
        var state = new TemperatureGateBoundUserInterfaceState(
            Name(uid),
            comp.Threshold,
            comp.Inverted,
            comp.Enabled);

        _ui.SetUiState(uid, TemperatureGateUiKey.Key, state);
    }
}
