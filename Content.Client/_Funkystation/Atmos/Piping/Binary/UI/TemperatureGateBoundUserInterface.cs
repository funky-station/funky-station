// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Funkystation.Atmos.Piping.Binary.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Atmos.Piping.Binary.UI;

/// <summary>
/// Bound user interface handler for the temperature gate device.
/// Manages opening the UI window and syncing state from server.
/// </summary>

[UsedImplicitly]
public sealed class TemperatureGateBoundUserInterface : BoundUserInterface
{
    private TemperatureGateWindow? _window;

    public TemperatureGateBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<TemperatureGateWindow>();

        _window.OnStatusToggled += (bool enabled) =>
        {
            SendMessage(new TemperatureGateToggleEnabledMessage { Enabled = !enabled });
        };

        _window.OnThresholdAndModeSet += (threshold, isMinMode) =>
        {
            SendMessage(new TemperatureGateSetThresholdAndModeMessage { Threshold = threshold, IsMinMode = isMinMode });
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not TemperatureGateBoundUserInterfaceState castState)
            return;

        _window.Title = castState.DeviceName;

        _window.UpdateUI(castState.Enabled, castState.Threshold, castState.IsMinMode);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _window?.Dispose();
        _window = null;
    }
}
