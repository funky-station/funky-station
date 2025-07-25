// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Maps;
using Robust.Shared.Noise;
using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.DungeonGenerators;

/// <summary>
/// Replaces existing tiles if they're not empty.
/// </summary>
public sealed partial class ReplaceTileDunGen : IDunGenLayer
{
    /// <summary>
    /// Chance for a non-variant tile to be used, in case they're too noisy.
    /// </summary>
    [DataField]
    public float VariantWeight = 0.1f;

    [DataField(required: true)]
    public List<ReplaceTileLayer> Layers = new();
}

[DataRecord]
public partial record struct ReplaceTileLayer
{
    public ProtoId<ContentTileDefinition> Tile;

    public float Threshold;

    public FastNoiseLite Noise;
}
