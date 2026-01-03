// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Zombies;

/// <summary>
/// Component that tracks CBURN shuttle spawn state for zombie outbreaks.
/// </summary>
[RegisterComponent]
public sealed partial class CBurnShuttleSpawnComponent : Component
{
    /// <summary>
    /// Whether CBURN shuttles have already been spawned for this round.
    /// </summary>
    [DataField]
    public bool HasSpawned { get; set; } = false;

    /// <summary>
    /// Set of shuttle UIDs that have been spawned to prevent duplicates.
    /// </summary>
    [DataField]
    public HashSet<EntityUid> SpawnedShuttles { get; set; } = new();
}
