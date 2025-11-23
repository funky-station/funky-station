// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later or MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Content.Shared.Maps;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Utility;

namespace Content.Server.Salvage;

/// <summary>
/// Generates ruins from station maps using cost-based flood-fill.
/// </summary>
public sealed class SalvageRuinGenerator
{
    private readonly IPrototypeManager _prototypeManager;
    private readonly IRobustRandom _random;
    private readonly ITileDefinitionManager _tileDefinitionManager;
    private readonly MapLoaderSystem _mapLoader;
    private readonly ISawmill _sawmill;

    public SalvageRuinGenerator(
        IPrototypeManager prototypeManager,
        IRobustRandom random,
        ITileDefinitionManager tileDefinitionManager,
        MapLoaderSystem mapLoader,
        ILogManager logManager)
    {
        _prototypeManager = prototypeManager;
        _random = random;
        _tileDefinitionManager = tileDefinitionManager;
        _mapLoader = mapLoader;
        _sawmill = logManager.GetSawmill("system.salvage");
    }

    /// <summary>
    /// Result of ruin generation containing floor tiles to place and wall entities to spawn.
    /// </summary>
    public sealed class RuinResult
    {
        public List<(Vector2i Position, Tile Tile)> FloorTiles = new();
        public List<(Vector2i Position, string PrototypeId)> WallEntities = new();
        public Box2 Bounds;
    }

    /// <summary>
    /// Generates a ruin from the specified map using flood-fill.
    /// Parses YAML directly without loading any entities.
    /// </summary>
    public RuinResult? GenerateRuin(ResPath mapPath, int seed, int floodFillPoints = 50)
    {
        // Read YAML file directly without loading entities
        if (!_mapLoader.TryReadFile(mapPath, out var mapData))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Failed to read map file: {mapPath}");
            return null;
        }

        // Parse tilemap section (maps YAML tile IDs to tile definition names)
        var tileMap = new Dictionary<int, string>();
        if (!mapData.TryGet("tilemap", out MappingDataNode? tilemapNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing tilemap section");
            return null;
        }

        foreach (var (key, valueNode) in tilemapNode.Children)
        {
            var valueValue = valueNode as ValueDataNode;
            if (valueValue == null)
                continue;

            if (!int.TryParse(key, out var yamlTileId))
                continue;

            tileMap[yamlTileId] = valueValue.Value;
        }

        if (tileMap.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} has empty tilemap");
            return null;
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Parsed {tileMap.Count} tile mappings from {mapPath}");

        // Parse grids section - contains entity UIDs, not grid data
        if (!mapData.TryGet("grids", out SequenceDataNode? gridsNode) || gridsNode.Sequence.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing or empty grids section");
            return null;
        }

        // Get the first grid UID
        var firstGridUidNode = gridsNode.Sequence[0] as ValueDataNode;
        if (firstGridUidNode == null || !int.TryParse(firstGridUidNode.Value, out var firstGridUid))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} first grid UID is invalid");
            return null;
        }

        // Parse entities section to find the grid entity
        if (!mapData.TryGet("entities", out SequenceDataNode? entitiesNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing entities section");
            return null;
        }

        // Find the grid entity by UID
        MappingDataNode? gridEntityNode = null;
        foreach (var protoGroupNode in entitiesNode.Sequence.Cast<MappingDataNode>())
        {
            if (!protoGroupNode.TryGet("entities", out SequenceDataNode? entitiesInGroup))
                continue;

            foreach (var entityNode in entitiesInGroup.Sequence.Cast<MappingDataNode>())
            {
                if (!entityNode.TryGet("uid", out ValueDataNode? uidNode))
                    continue;

                if (!int.TryParse(uidNode.Value, out var entityUid) || entityUid != firstGridUid)
                    continue;

                gridEntityNode = entityNode;
                break;
            }

            if (gridEntityNode != null)
                break;
        }

        if (gridEntityNode == null)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} grid entity with UID {firstGridUid} not found in entities section");
            return null;
        }

        // Get MapGridComponent from the entity
        if (!gridEntityNode.TryGet("components", out SequenceDataNode? componentsNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} grid entity missing components section");
            return null;
        }

        MappingDataNode? mapGridComponent = null;
        foreach (var componentNode in componentsNode.Sequence.Cast<MappingDataNode>())
        {
            if (!componentNode.TryGet("type", out ValueDataNode? typeNode))
                continue;

            if (typeNode.Value == "MapGrid")
            {
                mapGridComponent = componentNode;
                break;
            }
        }

        if (mapGridComponent == null)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} grid entity missing MapGrid component");
            return null;
        }

        // Get chunks from the MapGrid component (chunks are stored as a mapping, not a sequence)
        if (!mapGridComponent.TryGet("chunks", out MappingDataNode? chunksNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} MapGrid component missing chunks section");
            return null;
        }

        // Get chunk size (default 16)
        ushort chunkSize = 16;
        if (mapGridComponent.TryGet("chunksize", out ValueDataNode? chunkSizeNode))
        {
            if (ushort.TryParse(chunkSizeNode.Value, out var parsedChunkSize))
                chunkSize = parsedChunkSize;
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Parsing {chunksNode.Children.Count} chunks with chunk size {chunkSize}");

        // Build coordinate map from parsed chunks
        var coordinateMap = ParseChunks(chunksNode, tileMap, chunkSize);

        if (coordinateMap.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} produced empty coordinate map after parsing chunks");
            return null;
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Parsed {coordinateMap.Count} tiles from map {mapPath}");

            // Build cost map
            var costMap = BuildCostMap(coordinateMap);

            // Find valid start location (retry up to 10 times)
            var rand = new System.Random(seed);
            var startPos = FindValidStartLocation(costMap, rand, maxRetries: 10);
            if (!startPos.HasValue)
            {
                _sawmill.Error($"[SalvageRuinGenerator] Failed to find valid start location for map {mapPath} with seed {seed}");
                return null;
            }

            _sawmill.Debug($"[SalvageRuinGenerator] Starting flood-fill at {startPos.Value} with budget {floodFillPoints}");

            // Perform flood-fill
            var region = FloodFillWithCost(costMap, startPos.Value, floodFillPoints);
            if (region.Count == 0)
            {
                _sawmill.Error($"[SalvageRuinGenerator] Flood-fill returned empty region for map {mapPath} with seed {seed}");
                return null;
            }

            _sawmill.Debug($"[SalvageRuinGenerator] Flood-fill collected {region.Count} tiles");

            // Parse wall entities from the map
            var wallEntities = ParseWallEntities(entitiesNode, firstGridUid);

            // Extract floor tiles from region, and walls adjacent to the region
            var result = new RuinResult();
            var tilesToPlace = new Dictionary<Vector2i, Tile>();
            var minX = int.MaxValue;
            var minY = int.MaxValue;
            var maxX = int.MinValue;
            var maxY = int.MinValue;

            // First, find the bounds of the flood-filled region
            foreach (var pos in region)
            {
                minX = Math.Min(minX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxX = Math.Max(maxX, pos.X);
                maxY = Math.Max(maxY, pos.Y);
            }

            // Find wall entities adjacent to the flood-filled region
            var adjacentWallEntities = new List<(Vector2i Position, string PrototypeId)>();
            var regionSet = new HashSet<Vector2i>(region);
            
            foreach (var (wallPos, wallProto) in wallEntities)
            {
                // Check if this wall is adjacent to any tile in the region
                var neighbors = new[]
                {
                    new Vector2i(wallPos.X + 1, wallPos.Y),
                    new Vector2i(wallPos.X - 1, wallPos.Y),
                    new Vector2i(wallPos.X, wallPos.Y + 1),
                    new Vector2i(wallPos.X, wallPos.Y - 1)
                };

                foreach (var neighbor in neighbors)
                {
                    if (regionSet.Contains(neighbor))
                    {
                        adjacentWallEntities.Add((wallPos, wallProto));
                        // Update bounds to include walls
                        minX = Math.Min(minX, wallPos.X);
                        minY = Math.Min(minY, wallPos.Y);
                        maxX = Math.Max(maxX, wallPos.X);
                        maxY = Math.Max(maxY, wallPos.Y);
                        break; // Only add once
                    }
                }
            }

            _sawmill.Debug($"[SalvageRuinGenerator] Found {adjacentWallEntities.Count} adjacent wall entities");

            // Now normalize all coordinates to start from (0,0) relative to the ruin's origin
            var originX = minX;
            var originY = minY;

            // Add all tiles from the flood-filled region (floors) with normalized coordinates
            foreach (var pos in region)
            {
                if (!coordinateMap.TryGetValue(pos, out var tileId))
                    continue;

                // Get tile definition
                if (!_prototypeManager.TryIndex<ContentTileDefinition>(tileId, out var tileDef))
                    continue;

                // Convert to Tile
                var tile = new Tile(tileDef.TileId);
                // Normalize coordinates relative to ruin origin
                var normalizedPos = new Vector2i(pos.X - originX, pos.Y - originY);
                tilesToPlace[normalizedPos] = tile;
            }

            // Add adjacent wall entities with normalized coordinates
            foreach (var (wallPos, wallProto) in adjacentWallEntities)
            {
                var normalizedWallPos = new Vector2i(wallPos.X - originX, wallPos.Y - originY);
                result.WallEntities.Add((normalizedWallPos, wallProto));
            }

            if (tilesToPlace.Count == 0)
            {
                _sawmill.Error($"[SalvageRuinGenerator] No tiles to place after processing region for map {mapPath}");
                return null;
            }

            // Calculate normalized bounds (should start from 0,0)
            var normalizedMinX = 0;
            var normalizedMinY = 0;
            var normalizedMaxX = maxX - originX;
            var normalizedMaxY = maxY - originY;

            _sawmill.Debug($"[SalvageRuinGenerator] Generated ruin with {tilesToPlace.Count} tiles ({region.Count} floors) and {adjacentWallEntities.Count} wall entities");

            // Convert to list
            result.FloorTiles = tilesToPlace.Select(kvp => (kvp.Key, kvp.Value)).ToList();

            // Calculate bounds (add 1 for inclusive bounds)
            result.Bounds = new Box2(normalizedMinX, normalizedMinY, normalizedMaxX + 1, normalizedMaxY + 1);

            return result;
    }

    /// <summary>
    /// Parses chunks from YAML data and builds a coordinate map of tile positions to tile IDs.
    /// Chunks are stored as a mapping where keys are chunk index strings like "0,-1".
    /// </summary>
    private Dictionary<Vector2i, string> ParseChunks(MappingDataNode chunksNode, Dictionary<int, string> tileMap, ushort chunkSize)
    {
        var coordinateMap = new Dictionary<Vector2i, string>();

        foreach (var (chunkIndexKey, chunkValueNode) in chunksNode.Children)
        {
            // The key is the chunk index string (e.g., "0,-1")
            var chunkIndexStr = chunkIndexKey;
            var chunkIndexParts = chunkIndexStr.Split(',');
            if (chunkIndexParts.Length != 2 ||
                !int.TryParse(chunkIndexParts[0], out var chunkX) ||
                !int.TryParse(chunkIndexParts[1], out var chunkY))
            {
                _sawmill.Warning($"[SalvageRuinGenerator] Invalid chunk index format: {chunkIndexStr}");
                continue;
            }

            // The value is a mapping containing "ind", "tiles", and "version"
            if (chunkValueNode is not MappingDataNode chunkNode)
            {
                _sawmill.Warning($"[SalvageRuinGenerator] Chunk value is not a mapping node for chunk {chunkIndexStr}");
                continue;
            }

            // Get tile data (base64 encoded)
            if (!chunkNode.TryGet("tiles", out ValueDataNode? tilesNode))
                continue;

            // Get version (default 7 for newer maps)
            int version = 7;
            if (chunkNode.TryGet("version", out ValueDataNode? versionNode))
                int.TryParse(versionNode.Value, out version);

            // Decode base64 tile data
            byte[] tileBytes;
            try
            {
                tileBytes = Convert.FromBase64String(tilesNode.Value);
            }
            catch
            {
                continue;
            }

            using var stream = new MemoryStream(tileBytes);
            using var reader = new BinaryReader(stream);

            // Read tiles from the chunk
            for (ushort y = 0; y < chunkSize; y++)
            {
                for (ushort x = 0; x < chunkSize; x++)
                {
                    int yamlTileId;
                    byte flags;
                    byte variant;
                    byte rotationMirroring = 0;

                    if (version >= 7)
                    {
                        yamlTileId = reader.ReadInt32();
                        flags = reader.ReadByte();
                        variant = reader.ReadByte();
                        rotationMirroring = reader.ReadByte();
                    }
                    else
                    {
                        yamlTileId = version < 6 ? reader.ReadUInt16() : reader.ReadInt32();
                        flags = reader.ReadByte();
                        variant = reader.ReadByte();
                    }

                    // Map YAML tile ID to tile definition name
                    if (!tileMap.TryGetValue(yamlTileId, out var tileDefName))
                        continue;

                    // Convert to runtime tile ID
                    if (!_tileDefinitionManager.TryGetDefinition(tileDefName, out var tileDef))
                        continue;

                    // Calculate world position
                    var worldX = chunkX * chunkSize + x;
                    var worldY = chunkY * chunkSize + y;
                    var worldPos = new Vector2i(worldX, worldY);

                    // Store in coordinate map (only non-empty tiles)
                    if (tileDef.TileId != 0) // 0 is empty/space
                    {
                        coordinateMap[worldPos] = tileDef.ID;
                    }
                }
            }
        }

        return coordinateMap;
    }

    /// <summary>
    /// Parses wall entities from the map file and returns their positions and prototype IDs.
    /// </summary>
    private List<(Vector2i Position, string PrototypeId)> ParseWallEntities(SequenceDataNode entitiesNode, int gridUid)
    {
        var wallEntities = new List<(Vector2i, string)>();

        // Wall prototype IDs to look for
        var wallPrototypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "WallSolid",
            "WallReinforced",
            "WallReinforcedRust",
            "WallSolidRust"
        };

        foreach (var protoGroupNode in entitiesNode.Sequence.Cast<MappingDataNode>())
        {
            // Get the prototype ID for this group
            if (!protoGroupNode.TryGet("proto", out ValueDataNode? protoNode))
                continue;

            var protoId = protoNode.Value;
            
            // Skip if not a wall prototype
            if (!wallPrototypes.Contains(protoId))
                continue;

            if (!protoGroupNode.TryGet("entities", out SequenceDataNode? entitiesInGroup))
                continue;

            foreach (var entityNode in entitiesInGroup.Sequence.Cast<MappingDataNode>())
            {
                // Skip the grid entity itself
                if (entityNode.TryGet("uid", out ValueDataNode? uidNode) &&
                    int.TryParse(uidNode.Value, out var entityUid) &&
                    entityUid == gridUid)
                    continue;

                // Get Transform component to find position
                if (!entityNode.TryGet("components", out SequenceDataNode? componentsNode))
                    continue;

                Vector2i? entityPos = null;
                int? parentUid = null;

                foreach (var componentNode in componentsNode.Sequence.Cast<MappingDataNode>())
                {
                    if (!componentNode.TryGet("type", out ValueDataNode? typeNode))
                        continue;

                    if (typeNode.Value == "Transform")
                    {
                        // Get position
                        if (componentNode.TryGet("pos", out ValueDataNode? posNode))
                        {
                            var posParts = posNode.Value.Split(',');
                            if (posParts.Length == 2 &&
                                float.TryParse(posParts[0], out var x) &&
                                float.TryParse(posParts[1], out var y))
                            {
                                // Convert to tile coordinates (assuming 1 tile = 1 unit)
                                entityPos = new Vector2i((int)Math.Floor(x), (int)Math.Floor(y));
                            }
                        }

                        // Get parent (should be the grid)
                        if (componentNode.TryGet("parent", out ValueDataNode? parentNode) &&
                            int.TryParse(parentNode.Value, out var parent))
                        {
                            parentUid = parent;
                        }
                    }
                }

                // Only include entities that are children of the grid
                if (entityPos.HasValue && parentUid == gridUid)
                {
                    wallEntities.Add((entityPos.Value, protoId));
                }
            }
        }

        return wallEntities;
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
    /// Checks if a tile is a wall based on its prototype ID.
    /// </summary>
    private bool IsWallTile(string tileId)
    {
        return tileId.Equals("WallSolid", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallReinforced", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallReinforcedRust", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallSolidRust", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the cost for a tile based on its prototype ID.
    /// </summary>
    private int GetTileCost(string tileId)
    {
        // Walls: cost 6
        if (IsWallTile(tileId))
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

