// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// This is used to apply an offset to the star mark that shows up at cult tier 3.
/// </summary>
[RegisterComponent]
public sealed partial class CosmicStarMarkOffsetComponent : Component
{
    [DataField]
    public Vector2 Offset = Vector2.Zero;
}
