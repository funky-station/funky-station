using Robust.Shared.Serialization;

namespace Content.Shared.Objectives.Events;

[Serializable, NetSerializable]
public sealed class PlayerProximityEvent : EntityEventArgs
{
    public PlayerProximityEvent()
    {
    }
}
