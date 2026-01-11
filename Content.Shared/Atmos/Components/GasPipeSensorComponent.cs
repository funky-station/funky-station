// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using static Content.Shared.Atmos.Components.GasAnalyzerComponent;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// Entities with component will be queried against for their
/// atmos monitoring data on atmos monitoring consoles
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GasPipeSensorComponent : Component;


// FunkyStation: Pressure states for pipe sensors and built in analyzer
[RegisterComponent]
public sealed partial class ActiveGasPipeSensorComponent : Component
{
    [DataField("accumulatedFrameTime")]
    public float AccumulatedFrameTime { get; set; } = 0f;

    [DataField("updateInterval")]
    public float UpdateInterval { get; set; } = 1f;
}

[Serializable, NetSerializable]
public enum GasPipeSensorVisuals : byte
{
    LightsState,
}

[Serializable, NetSerializable]
public enum PipeLightsState : byte
{
    Off,
    NormalPressure,
    OverPressure,
    ExtremePressure
}

[Serializable, NetSerializable]
public enum GasPipeSensorUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class GasPipeSensorUserMessage : BoundUserInterfaceMessage
{
    public readonly GasMixEntry? PipeMix;
    public readonly string? PipeNodeName;
    public readonly float TotalPressure;
    public readonly float Temperature;
    public string? Error;

    public GasPipeSensorUserMessage(
        GasMixEntry? pipeMix,
        string? pipeNodeName,
        float totalPressure,
        float temperature,
        string? error = null)
    {
        PipeMix = pipeMix;
        PipeNodeName = pipeNodeName;
        TotalPressure = totalPressure;
        Temperature = temperature;
        Error = error;
    }
}
