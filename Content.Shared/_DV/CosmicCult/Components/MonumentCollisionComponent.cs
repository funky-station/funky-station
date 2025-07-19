// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for handling The Monument's collision.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MonumentCollisionComponent : Component
{
    /// <summary>
    /// Determines whether The Monument is tangible to non-cultists.
    /// </summary>
    [DataField, AutoNetworkedField] public bool HasCollision;
}
