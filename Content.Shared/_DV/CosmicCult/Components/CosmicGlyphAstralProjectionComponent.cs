// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicGlyphAstralProjectionComponent : Component
{
    [DataField]
    public EntProtoId SpawnProjection = "MobCosmicAstralProjection";

    /// <summary>
    /// The duration of the astral projection
    /// </summary>
    [DataField]
    public TimeSpan AstralDuration = TimeSpan.FromSeconds(12);

    [DataField]
    public DamageSpecifier ProjectionDamage = new()
    {
        DamageDict = new() {
            { "Asphyxiation", 40 }
        }
    };
}
