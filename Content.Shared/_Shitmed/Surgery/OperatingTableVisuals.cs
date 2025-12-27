using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Surgery;

[Serializable, NetSerializable]
public enum OperatingTableVisuals : byte
{
    LightOn,
    VitalsState,
}

[Serializable, NetSerializable]
public enum OperatingTableVisualLayers : byte
{
    Base,
    Vitals,
}

[Serializable, NetSerializable]
public enum VitalsState : byte
{
    None,
    Healthy,
    Injured,
    Critical,
    Dead,
}
