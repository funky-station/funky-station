using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum NuclearReactorUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class NuclearReactorBuiState : BoundUserInterfaceState
{
    public double[] TemperatureGrid = new double[NuclearReactorComponent.ReactorGridWidth*NuclearReactorComponent.ReactorGridHeight];
    public int[] NeutronGrid = new int[NuclearReactorComponent.ReactorGridWidth*NuclearReactorComponent.ReactorGridHeight];
    public string[] IconGrid = new string[NuclearReactorComponent.ReactorGridWidth * NuclearReactorComponent.ReactorGridHeight];
}

[Serializable, NetSerializable]
public sealed class ReactorChangeViewMessage(byte view) : BoundUserInterfaceMessage
{
    public byte FlowRate { get; } = view;
}