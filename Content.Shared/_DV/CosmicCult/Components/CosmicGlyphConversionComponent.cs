// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
///     Indicates a glyph entity as performing conversion effects
/// </summary>
[RegisterComponent]
public sealed partial class CosmicGlyphConversionComponent : Component
{
    /// <summary>
    ///     The search range for finding conversion targets.
    /// </summary>
    [DataField]
    public float ConversionRange = 0.5f;

    /// <summary>
    ///     Whether or not we ignore mindshields or chaplain status.
    /// </summary>
    [DataField]
    public bool NegateProtection;

    /// <summary>
    ///     Healing applied on conversion.
    /// </summary>
    [DataField]
    public DamageSpecifier ConversionHeal = new()
    {
        DamageDict = new()
        {
            { "Blunt", 50},
            { "Slash", 50},
            { "Piercing", 50},
            { "Heat", 50},
            { "Shock", 50},
            { "Cold", 50},
            { "Poison", 50},
            { "Radiation", 50},
            { "Asphyxiation", 50}
        }
    };
}
