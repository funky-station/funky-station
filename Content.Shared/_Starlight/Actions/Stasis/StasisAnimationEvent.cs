// SPDX-FileCopyrightText: 2025 SigmaTheDragon <162711378+SigmaTheDragon@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using Robust.Shared.Map;

namespace Content.Shared._Starlight.Actions.Stasis;

/// <summary>
/// # Starlight, under MIT License
/// The type of stasis animation to play
/// </summary>
[Serializable, NetSerializable]
public enum StasisAnimationType
{
    /// <summary>
    /// Animation played when preparing stasis
    /// </summary>
    Prepare,

    /// <summary>
    /// Animation played when entering stasis
    /// </summary>
    Enter,

    /// <summary>
    /// Animation played when exiting stasis
    /// </summary>
    Exit
}

/// <summary>
/// Network event for playing the stasis animation on all clients
/// </summary>
[Serializable, NetSerializable]
public sealed class StasisAnimationEvent : EntityEventArgs
{
    public NetEntity Entity;
    public NetCoordinates Coordinates;
    public StasisAnimationType AnimationType;

    public StasisAnimationEvent(NetEntity entity, NetCoordinates coordinates, StasisAnimationType animationType)
    {
        Entity = entity;
        Coordinates = coordinates;
        AnimationType = animationType;
    }
}
