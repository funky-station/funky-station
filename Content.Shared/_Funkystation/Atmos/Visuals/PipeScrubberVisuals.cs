using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Atmos.Visuals;

[Serializable, NetSerializable]
public enum PipeScrubberVisuals : byte
{
    IsFull,
    IsEnabled,
    IsScrubbing,
    IsDraining,
}
