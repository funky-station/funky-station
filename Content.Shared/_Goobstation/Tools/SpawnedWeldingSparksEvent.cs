// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Tools;

/// <summary>
/// Raised by <c>WeldingSparksSystem</c> after it's spawned the sparks effect if there was a target to spawn them on.
/// </summary>
/// <param name="targetEnt">The entity being welded.</param>
/// <param name="sparksEnt">The welding sparks effect entity.</param>
/// <param name="duration">How long should the animation take to complete.</param>
[Serializable, NetSerializable]
public sealed partial class SpawnedWeldingSparksEvent(NetEntity targetEnt, NetEntity sparksEnt, TimeSpan duration) : EntityEventArgs
{
    public NetEntity TargetEnt = targetEnt;
    public NetEntity SparksEnt = sparksEnt;
    public TimeSpan Duration = duration;
}
