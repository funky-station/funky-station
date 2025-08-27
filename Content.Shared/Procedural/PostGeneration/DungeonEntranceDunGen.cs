// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Selects [count] rooms and places external doors to them.
/// </summary>
/// <remarks>
/// Dungeon data keys are:
/// - Entrance
/// - FallbackTile
/// </remarks>
public sealed partial class DungeonEntranceDunGen : IDunGenLayer
{
    /// <summary>
    /// How many rooms we place doors on.
    /// </summary>
    [DataField]
    public int Count = 1;
}
