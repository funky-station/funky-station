// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;

namespace Content.Server.Antag.Components;

/// <summary>
/// Spawns this rule's antags at random tiles on a station using <c>TryGetRandomTile</c>.
/// Requires <see cref="AntagSelectionComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class AntagRandomSpawnComponent : Component
{
    /// <summary>
    /// Location that was picked.
    /// </summary>
    [DataField]
    public EntityCoordinates? Coords;
}
