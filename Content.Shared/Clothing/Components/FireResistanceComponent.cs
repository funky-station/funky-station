// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing.EntitySystems;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Makes this clothing reduce fire damage when worn.
/// </summary>
[RegisterComponent, Access(typeof(FireResistanceSystem))]
public sealed partial class FireResistanceComponent : Component
{
    /// <summary>
    ///     The fern resistance coefficient, This fraction is multiplied into the total resistance.
    /// </summary>
    [DataField("damageCoefficient")]
    public float DamageCoefficient = 1;

    /// <summary>
    /// LocId for message that will be shown on detailed examine.
    /// Actually can be moved into system
    /// </summary>
    [DataField]
    public LocId ExamineMessage = "fire-protection-reduction-value";
}
