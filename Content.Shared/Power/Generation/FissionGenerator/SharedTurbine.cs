using Robust.Shared.Serialization;

namespace Content.Shared.Power.Generation.FissionGenerator;

[Serializable, NetSerializable]
public enum TurbineVisuals
{
    TurbineRuined,
    DamageSpark,
    DamageSmoke,
    TurbineSpeed,
}

[Serializable, NetSerializable]
public enum TurbineVisualLayers
{
    TurbineRuined,
    DamageSpark,
    DamageSmoke,
    TurbineSpeed,
}

[Serializable, NetSerializable]
public enum TurbineSpeed
{
    SpeedStill,
    SpeedSlow,
    SpeedMid,
    SpeedFast,
    SpeedOverspeed,
}