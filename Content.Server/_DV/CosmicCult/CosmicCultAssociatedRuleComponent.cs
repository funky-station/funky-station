// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server._DV.CosmicCult;

/// <summary>
///     Associates an entity with a specific cosmic cult gamerule
/// </summary>
[RegisterComponent]
public sealed partial class CosmicCultAssociatedRuleComponent : Component
{
    /// <summary>
    ///     The gamerule that this entity is associated with
    /// </summary>
    [DataField]
    public EntityUid CultGamerule;
}
