using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Piping.Binary.Components;

[Serializable, NetSerializable]
public enum TemperatureGateUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class TemperatureGateSetThresholdAndModeMessage : BoundUserInterfaceMessage
{
    public float Threshold { get; }
    public bool IsMinMode { get; }

    public TemperatureGateSetThresholdAndModeMessage(float threshold, bool isMinMode)
    {
        Threshold = threshold;
        IsMinMode = isMinMode;
    }
}

[Serializable, NetSerializable]
public sealed class TemperatureGateToggleEnabledMessage : BoundUserInterfaceMessage
{
    public bool Enabled { get; }

    public TemperatureGateToggleEnabledMessage(bool enabled) => Enabled = enabled;
}

[Serializable, NetSerializable]
public sealed class TemperatureGateBoundUserInterfaceState : BoundUserInterfaceState
{
    public string DeviceName { get; }
    public float Threshold { get; }
    public bool IsMinMode { get; }
    public bool Enabled { get; }

    public TemperatureGateBoundUserInterfaceState(string deviceName, float threshold, bool isMinMode, bool enabled)
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
    State
}

[Serializable, NetSerializable]
public enum TemperatureGateState : byte
{
    Off,
    On,
    Flow
}
