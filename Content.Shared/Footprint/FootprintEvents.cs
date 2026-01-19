using Robust.Shared.Serialization;

namespace Content.Shared.Footprint;

[Serializable, NetSerializable]
public sealed class FootprintChangedEvent(NetEntity entity) : EntityEventArgs
{
    public NetEntity Entity = entity;
}

public readonly struct FootprintCleanEvent;
