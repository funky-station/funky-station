// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Content.Shared.Procedural;

// TODO: Cache center and bounds and shit and don't make the caller deal with it.
public sealed record DungeonRoom(HashSet<Vector2i> Tiles, Vector2 Center, Box2i Bounds, HashSet<Vector2i> Exterior)
{
    public readonly List<Vector2i> Entrances = new();

    /// <summary>
    /// Nodes adjacent to tiles, including the corners.
    /// </summary>
    public readonly HashSet<Vector2i> Exterior = Exterior;
}
