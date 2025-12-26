using Robust.Shared.Serialization;

namespace Content.Shared.StationEvents.Events;

[Serializable, NetSerializable]
public sealed partial class SolarFlareStartedEvent(NetEntity gamerule) : EntityEventArgs
{
    public readonly NetEntity Gamerule = gamerule;
}
