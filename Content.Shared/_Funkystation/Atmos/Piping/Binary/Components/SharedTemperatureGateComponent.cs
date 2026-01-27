// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Atmos;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Piping.Binary.Components;

/// <summary>
///     A binary atmos device that acts as a temperature-controlled valve.
///     Allows gas to flow from inlet to outlet only when the inlet temperature satisfies
///     the configured threshold condition and the device is enabled/powered.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TemperatureGateComponent : Component
{
    /// <summary>
    ///     Temperature threshold in Kelvin. The gate opens if inlet temperature meets
    ///     the condition defined by <see cref="Inverted"/>.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Threshold = Atmospherics.T20C;

    /// <summary>
    ///     Determines the comparison direction:
    ///     - false: opens when temperature < threshold (max mode)
    ///     - true:  opens when temperature > threshold (min mode)
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Inverted;

    /// <summary>
    ///     Whether the gate is currently allowed to open.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Enabled;

    /// <summary>
    ///     Identifier of the inlet pipe node in the node's container.
    /// </summary>
    [DataField]
    public string Inlet = "inlet";

    /// <summary>
    ///     Identifier of the outlet pipe node in the node's container.
    /// </summary>
    [DataField]
    public string Outlet = "outlet";

    /// <summary>
    ///     Minimum allowed value for <see cref="Threshold"/> in Kelvin.
    /// </summary>
    [DataField]
    public float MinThreshold = 3f;

    /// <summary>
    ///     Maximum allowed value for <see cref="Threshold"/> in Kelvin.
    /// </summary>
    [DataField]
    public float MaxThreshold = 12000f;
}

[Serializable, NetSerializable]
public enum TemperatureGateUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class TemperatureGateSetThresholdAndModeMessage : BoundUserInterfaceMessage
{
    public float Threshold;
    public bool IsMinMode;
}

[Serializable, NetSerializable]
public sealed class TemperatureGateToggleEnabledMessage : BoundUserInterfaceMessage
{
    public bool Enabled;
}

[Serializable, NetSerializable]
public sealed class TemperatureGateBoundUserInterfaceState : BoundUserInterfaceState
{
    /// <summary>
    /// Friendly name of the device for UI title.
    /// </summary>
    public string DeviceName;

    /// <summary>
    /// Current temperature threshold in Kelvin.
    /// </summary>
    public float Threshold;

    /// <summary>
    /// True if in "minimum temperature" mode (opens when > threshold).
    /// </summary>
    public bool IsMinMode;

    /// <summary>
    /// Whether the gate is enabled.
    /// </summary>
    public bool Enabled;

    public TemperatureGateBoundUserInterfaceState(
        string deviceName,
        float threshold,
        bool isMinMode,
        bool enabled)
    {
        DeviceName = deviceName;
        Threshold = threshold;
        IsMinMode = isMinMode;
        Enabled = enabled;
    }
}

[Serializable, NetSerializable]
public enum TemperatureGateVisuals : byte
{
    State,
}

[Serializable, NetSerializable]
public enum TemperatureGateState : byte
{
    Off,
    On,
    Flow,
}
