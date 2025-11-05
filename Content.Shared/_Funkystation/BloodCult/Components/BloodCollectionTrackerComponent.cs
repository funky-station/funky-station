// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared.BloodCult.Components;

/// <summary>
/// Tracks how much blood has been collected from this entity for the Blood Cult ritual pool.
/// Used to prevent farming a single entity indefinitely.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BloodCollectionTrackerComponent : Component
{
    /// <summary>
    /// Total amount of blood collected from this entity so far.
    /// Capped at 100 units per entity.
    /// </summary>
    [DataField]
    public float TotalBloodCollected;

    /// <summary>
    /// Maximum amount of blood that can be collected from a single entity.
    /// </summary>
    [DataField]
    public float MaxBloodPerEntity = 100.0f;
}

