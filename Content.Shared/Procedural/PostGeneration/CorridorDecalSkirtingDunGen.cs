// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Decals;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Applies decal skirting to corridors.
/// </summary>
public sealed partial class CorridorDecalSkirtingDunGen : IDunGenLayer
{
    /// <summary>
    /// Decal where 1 edge is found.
    /// </summary>
    [DataField]
    public Dictionary<DirectionFlag, string> CardinalDecals = new();

    /// <summary>
    /// Decal where 1 corner edge is found.
    /// </summary>
    [DataField]
    public Dictionary<Direction, string> PocketDecals = new();

    /// <summary>
    /// Decal where 2 or 3 edges are found.
    /// </summary>
    [DataField]
    public Dictionary<DirectionFlag, string> CornerDecals = new();
}
