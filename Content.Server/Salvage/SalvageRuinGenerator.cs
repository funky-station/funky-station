// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Shared.Maps;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.Salvage;

/// <summary>
/// Generates ruins from station maps using cost-based flood-fill.
/// </summary>
public sealed class SalvageRuinGenerator
{
    private readonly IMapManager _mapManager;
    private readonly IPrototypeManager _prototypeManager;
    private readonly IRobustRandom _random;
    private readonly SharedMapSystem _mapSystem;
    private readonly ITileDefinitionManager _tileDefinitionManager;
    private readonly MapLoaderSystem _mapLoader;

    public SalvageRuinGenerator(
        IMapManager mapManager,
        IPrototypeManager prototypeManager,
        IRobustRandom random,
        SharedMapSystem mapSystem,
        ITileDefinitionManager tileDefinitionManager,
        MapLoaderSystem mapLoader)
    {
        _mapManager = mapManager;
        _prototypeManager = prototypeManager;
        _random = random;
        _mapSystem = mapSystem;
        _tileDefinitionManager = tileDefinitionManager;
        _mapLoader = mapLoader;
    }

    /// <summary>
    /// Result of ruin generation containing floor tiles to place.
    /// </summary>
    public sealed class RuinResult
    {
        public List<(Vector2i Position, Tile Tile)> FloorTiles = new();
        public Box2 Bounds;
    }

    /// <summary>
    /// Generates a ruin from the specified map using flood-fill.
    /// </summary>
    public RuinResult? GenerateRuin(ResPath mapPath, int seed, int floodFillPoints = 50)
    {
        // Load map temporarily to read tiles
        var tempMapUid = _mapSystem.CreateMap(out var tempMapId);
        Entity<MapGridComponent>? tempGrid = null;

        try
        {
            if (!_mapLoader.TryLoadGrid(tempMapId, mapPath, out tempGrid))
            {
                _mapSystem.DeleteMap(tempMapId);
                return null;
            }

            if (!tempGrid.HasValue)
            {
                _mapSystem.DeleteMap(tempMapId);
                return null;
            }

            var grid = tempGrid.Value.Comp;
            var gridUid = tempGrid.Value.Owner;

            // Build coordinate map with tile prototype IDs
            var coordinateMap = new Dictionary<Vector2i, string>();
            var allTiles = _mapSystem.GetAllTiles(gridUid, grid, ignoreEmpty: false);

            foreach (var tileRef in allTiles)
            {
                if (tileRef.Tile.IsEmpty)
                    continue;

                var tileDef = _tileDefinitionManager[tileRef.Tile.TypeId];
                coordinateMap[tileRef.GridIndices] = tileDef.ID;
            }

            if (coordinateMap.Count == 0)
                return null;

            // Build cost map
            var costMap = BuildCostMap(coordinateMap);

            // Find valid start location (retry up to 10 times)
            var rand = new System.Random(seed);
            var startPos = FindValidStartLocation(costMap, rand, maxRetries: 10);
            if (!startPos.HasValue)
                return null;

            // Perform flood-fill
            var region = FloodFillWithCost(costMap, startPos.Value, floodFillPoints);
            if (region.Count == 0)
                return null;

            // Extract floor tiles from region
            var result = new RuinResult();
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            foreach (var pos in region)
            {
                if (!coordinateMap.TryGetValue(pos, out var tileId))
                    continue;

                // Get tile definition
                if (!_prototypeManager.TryIndex<ContentTileDefinition>(tileId, out var tileDef))
                    continue;

                // Convert to Tile
                var tile = new Tile(tileDef.TileId);
                result.FloorTiles.Add((pos, tile));

                // Update bounds
                minX = Math.Min(minX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxX = Math.Max(maxX, pos.X);
                maxY = Math.Max(maxY, pos.Y);
            }

            if (result.FloorTiles.Count == 0)
                return null;

            // Calculate bounds (add 1 for inclusive bounds)
            result.Bounds = new Box2(minX, minY, maxX + 1, maxY + 1);

            return result;
        }
        finally
        {
            // Clean up temp map (deleting map will delete all grids on it)
            _mapSystem.DeleteMap(tempMapId);
        }
    }

    /// <summary>
    /// Builds a cost map from coordinate map. Space tiles (missing from map) get cost 99.
    /// </summary>
    private Dictionary<Vector2i, int> BuildCostMap(Dictionary<Vector2i, string> coordinateMap)
    {
        var costMap = new Dictionary<Vector2i, int>();

        foreach (var (pos, tileId) in coordinateMap)
        {
            var cost = GetTileCost(tileId);
            costMap[pos] = cost;
        }

        return costMap;
    }

    /// <summary>
    /// Gets the cost for a tile based on its prototype ID.
    /// </summary>
    private int GetTileCost(string tileId)
    {
        // Walls: cost 6
        if (tileId.Equals("WallSolid", StringComparison.OrdinalIgnoreCase) ||
            tileId.Equals("WallReinforced", StringComparison.OrdinalIgnoreCase) ||
            tileId.Equals("WallReinforcedRust", StringComparison.OrdinalIgnoreCase) ||
            tileId.Equals("WallSolidRust", StringComparison.OrdinalIgnoreCase))
        {
            return 6;
        }

        // Directional Glass: cost 2
        if (tileId.Contains("DirectionalGlass", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        // Reinforced Glass or Glass (excluding Directional): cost 4
        if (tileId.Contains("ReinforcedGlass", StringComparison.OrdinalIgnoreCase) ||
            (tileId.Contains("Glass", StringComparison.OrdinalIgnoreCase) &&
             !tileId.Contains("Directional", StringComparison.OrdinalIgnoreCase)))
        {
            return 4;
        }

        // Grilles: cost 2
        if (tileId.Contains("Grille", StringComparison.OrdinalIgnoreCase))
        {
            return 2;
        }

        // All others: cost 1
        return 1;
    }

    /// <summary>
    /// Finds a valid start location (non-space tile) with retries.
    /// </summary>
    private Vector2i? FindValidStartLocation(Dictionary<Vector2i, int> costMap, System.Random rand, int maxRetries)
    {
        if (costMap.Count == 0)
            return null;

        var positions = costMap.Keys.ToList();

        for (var i = 0; i < maxRetries; i++)
        {
            var pos = positions[rand.Next(positions.Count)];
            var cost = costMap[pos];

            // Space has cost 99, so any tile with cost < 99 is valid
            if (cost < 99)
                return pos;
        }

        // If we couldn't find a non-space tile after retries, just return first non-space if any
        foreach (var (pos, cost) in costMap)
        {
            if (cost < 99)
                return pos;
        }

        return null;
    }

    /// <summary>
    /// Performs cost-based flood-fill from start position, collecting up to budget points.
    /// </summary>
    private HashSet<Vector2i> FloodFillWithCost(Dictionary<Vector2i, int> costMap, Vector2i start, int budget)
    {
        var result = new HashSet<Vector2i>();
        var visited = new HashSet<Vector2i>();
        var queue = new List<(Vector2i Pos, int AccumulatedCost)>();
        var accumulatedCosts = new Dictionary<Vector2i, int>();

        queue.Add((start, 0));
        accumulatedCosts[start] = 0;

        while (queue.Count > 0 && result.Count < budget)
        {
            // Sort by accumulated cost (min-heap simulation)
            queue.Sort((a, b) => a.AccumulatedCost.CompareTo(b.AccumulatedCost));
            var (current, accumulatedCost) = queue[0];
            queue.RemoveAt(0);

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            // Check if this tile exists in cost map (not space)
            if (!costMap.TryGetValue(current, out var tileCost))
            {
                // Space tile, skip
                continue;
            }

            // Add to result
            result.Add(current);

            // Explore neighbors (4-directional)
            var neighbors = new[]
            {
                current + Vector2i.Up,
                current + Vector2i.Down,
                current + Vector2i.Left,
                current + Vector2i.Right
            };

            foreach (var neighbor in neighbors)
            {
                if (visited.Contains(neighbor))
                    continue;

                // Check if neighbor exists in cost map
                if (!costMap.TryGetValue(neighbor, out var neighborCost))
                {
                    // Space tile, skip
                    continue;
                }

                var newAccumulatedCost = accumulatedCost + neighborCost;

                // Only add if we haven't seen it or if this path is cheaper
                if (!accumulatedCosts.TryGetValue(neighbor, out var existingCost) ||
                    newAccumulatedCost < existingCost)
                {
                    accumulatedCosts[neighbor] = newAccumulatedCost;
                    queue.Add((neighbor, newAccumulatedCost));
                }
            }
        }

        return result;
    }
}

