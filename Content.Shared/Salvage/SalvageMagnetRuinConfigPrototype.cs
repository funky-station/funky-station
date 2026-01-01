// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Salvage;

/// <summary>
/// Configuration for salvage magnet ruin generation, including damage simulation parameters.
/// These are all overwritten by salvage_magnet_ruin_config.yml in the Resources/Prototypes/Procedural folder.
/// </summary>
[Prototype("salvageMagnetRuinConfig")]
public sealed partial class SalvageMagnetRuinConfigPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Number of cost points to use for flood-fill algorithm.
    /// Higher values result in larger ruins.
    /// </summary>
    [DataField(required: true)]
    public int FloodFillPoints = 50;

    /// <summary>
    /// Chance (0.0 to 1.0) that a wall entity will be destroyed and not spawned.
    /// </summary>
    [DataField]
    public float WallDestroyChance = 0.0f;

    /// <summary>
    /// Chance (0.0 to 1.0) that a window entity will be spawned in a damaged state.
    /// </summary>
    [DataField]
    public float WindowDamageChance = 0.0f;

    /// <summary>
    /// Chance (0.0 to 1.0) that a floor tile will be replaced with lattice.
    /// Lattice tiles are never damaged further (they're already the most damaged state).
    /// </summary>
    [DataField]
    public float FloorToLatticeChance = 0.0f;

    /// <summary>
    /// Path cost for walls. Higher values make flood-fill avoid walls more.
    /// </summary>
    [DataField]
    public int WallCost = 6;

    /// <summary>
    /// Path cost for directional windows. Lower values make flood-fill prefer directional windows.
    /// </summary>
    [DataField]
    public int DirectionalWindowCost = 2;

    /// <summary>
    /// Path cost for reinforced windows. Higher values make flood-fill avoid reinforced windows.
    /// </summary>
    [DataField]
    public int ReinforcedWindowCost = 4;

    /// <summary>
    /// Path cost for regular (non-directional, non-reinforced) windows.
    /// </summary>
    [DataField]
    public int RegularWindowCost = 2;

    /// <summary>
    /// Path cost for directional glass tiles.
    /// </summary>
    [DataField]
    public int DirectionalGlassCost = 2;

    /// <summary>
    /// Path cost for reinforced glass tiles.
    /// </summary>
    [DataField]
    public int ReinforcedGlassCost = 4;

    /// <summary>
    /// Path cost for regular glass tiles (non-directional, non-reinforced).
    /// </summary>
    [DataField]
    public int RegularGlassCost = 4;

    /// <summary>
    /// Path cost for grille tiles.
    /// </summary>
    [DataField]
    public int GrilleCost = 2;

    /// <summary>
    /// Default path cost for all other tiles (floors, etc.).
    /// </summary>
    [DataField]
    public int DefaultTileCost = 1;

    /// <summary>
    /// Path cost for space tiles (tiles not in the map).
    /// Set to a very high value (99) to make flood-fill treat them as impassable.
    /// Lower values would allow flood-fill to cross small gaps of space.
    /// </summary>
    [DataField]
    public int SpaceCost = 9999;

    /// <summary>
    /// Number of flood-fill stages to perform. Each stage starts from the previous stage's frontier
    /// (tiles that were almost added but exceeded budget), creating irregular branching shapes.
    /// </summary>
    [DataField]
    public int FloodFillStages = 5;

    /// <summary>
    /// Distance (in tiles) at which ruins spawn from the salvage magnet.
    /// Ruins spawn this distance away in the direction the magnet is facing.
    /// </summary>
    [DataField]
    public float RuinSpawnDistance = 64f;
}

