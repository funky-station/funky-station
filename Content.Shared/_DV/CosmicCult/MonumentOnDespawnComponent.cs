// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult;

/// <summary>
/// When a <c>MonumentOnDespawnComponent</c> despawns, a monument will spawn in its place with the same cult association
/// </summary>
[RegisterComponent, Access(typeof(SharedMonumentSystem))]
public sealed partial class MonumentOnDespawnComponent : Component
{
    /// <summary>
    /// Entity prototype to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Prototype = string.Empty;
}
