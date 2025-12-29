// SPDX-License-Identifier: MIT

using Robust.Shared.Serialization;

namespace Content.Shared.Weapons.Ranged.Events;

/// <summary>
/// Raised when a battery weapon's fire mode is changed.
/// Used to notify systems (like prediction) that the projectile prototype has changed.
/// </summary>
[ByRefEvent]
public record struct FireModeChangedEvent(string NewPrototype);

/// <summary>
/// Sent from client to server to request a fire mode change.
/// This ensures the change is predicted on the client.
/// </summary>
[Serializable, NetSerializable]
public sealed class RequestFireModeChangeEvent : EntityEventArgs
{
    public NetEntity Gun;
    public int Index;

    public RequestFireModeChangeEvent(NetEntity gun, int index)
    {
        Gun = gun;
        Index = index;
    }
}
