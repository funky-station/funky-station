// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._RMC14.Weapons.Ranged.Prediction;

[Serializable, NetSerializable]
public sealed class PredictedProjectileHitEvent(int projectile, HashSet<(NetEntity Id, MapCoordinates Coordinates)> hit) : EntityEventArgs
{
    public readonly int Projectile = projectile;
    public readonly HashSet<(NetEntity Id, MapCoordinates Coordinates)> Hit = hit;
}
