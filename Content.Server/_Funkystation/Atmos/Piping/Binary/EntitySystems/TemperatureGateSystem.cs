// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.NodeContainer.EntitySystems;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Power.Components;
using Content.Shared._Funkystation.Atmos.Piping.Binary.Components;
using Content.Shared._Funkystation.Atmos.Piping.Binary.EntitySystems;
using Content.Shared.Audio;

namespace Content.Server._Funkystation.Atmos.Piping.Binary.EntitySystems;

public sealed class TemperatureGateSystem : SharedTemperatureGateSystem
{
    [Dependency] private readonly SharedAmbientSoundSystem _ambientSoundSystem = default!;
    [Dependency] private readonly NodeContainerSystem _nodeContainer = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TemperatureGateComponent, AtmosDeviceUpdateEvent>(OnAtmosUpdate);
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
            uid, comp.Inlet, comp.Outlet,
            out var inlet, out var outlet))
        {
            return;
        }

        // Check if temperature condition allows flow
        var shouldFlow = comp.Inverted
            ? inlet.Air.Temperature >= comp.Threshold
            : inlet.Air.Temperature <= comp.Threshold;

        if (!shouldFlow)
        {
            UpdateAppearance(uid, comp, isOn: true); // Can just return true for isOn as power check was handled above
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        var p1 = inlet.Air.Pressure;
        var p2 = outlet.Air.Pressure;

        // No flow if pressure is not pushing from inlet to outlet
        if (p1 <= p2)
        {
            UpdateAppearance(uid, comp, isOn: true, isFlowing: false);
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
            UpdateAppearance(uid, comp, isOn: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        // This is the exact number of moles that would equalize pressure between the two sides
        // if transferred all at once, accounting for their different temperatures and volumes.
        var transferMoles = n1 - (n1 + n2) * t2 * v1 / denom;

        if (transferMoles <= 0f)
        {
            UpdateAppearance(uid, comp, isOn: true, isFlowing: false);
            _ambientSoundSystem.SetAmbience(uid, false);
            return;
        }

        // Remove transfer amount from inlet and merge into outlet
        var removed = inlet.Air.Remove(transferMoles);
        _atmosphere.Merge(outlet.Air, removed);

        UpdateAppearance(uid, comp, isOn: true, isFlowing: true);
        _ambientSoundSystem.SetAmbience(uid, true);
    }
}
