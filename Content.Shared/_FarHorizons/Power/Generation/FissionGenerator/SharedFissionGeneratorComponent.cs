using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum FissionGeneratorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class FissionGeneratorBuiState : BoundUserInterfaceState
{
    public double[] TemperatureGrid = new double[FissionGeneratorComponent.ReactorGridWidth*FissionGeneratorComponent.ReactorGridHeight];
    public int[] NeutronGrid = new int[FissionGeneratorComponent.ReactorGridWidth*FissionGeneratorComponent.ReactorGridHeight];
    public string[] IconGrid = new string[FissionGeneratorComponent.ReactorGridWidth * FissionGeneratorComponent.ReactorGridHeight];
}

[Serializable, NetSerializable]
public sealed class ReactorChangeViewMessage(byte view) : BoundUserInterfaceMessage
{
    public byte FlowRate { get; } = view;
}