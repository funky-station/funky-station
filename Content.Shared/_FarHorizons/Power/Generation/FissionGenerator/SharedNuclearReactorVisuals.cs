using Robust.Shared.Serialization;

namespace Content.Shared._FarHorizons.Power.Generation.FissionGenerator;

#region Reactor Caps
[Serializable, NetSerializable]
public enum ReactorCapVisuals
{
    Sprite
}

[Serializable, NetSerializable]
public enum ReactorCapVisualLayers
{
    Sprite
}

[Serializable, NetSerializable]
public enum ReactorCaps
{
    Base,

    Control,
    ControlM1,
    ControlM2,
    ControlM3,
    ControlM4,

    Fuel,
    FuelM1,
    FuelM2, 
    FuelM3,
    FuelM4,

    Gas,
    GasM1,
    GasM2,
    GasM3,
    GasM4,

    Heat,
    HeatM1,
    HeatM2,
    HeatM3,
    HeatM4,
}
#endregion