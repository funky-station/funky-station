// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using Content.Shared.Maps;
using Content.Shared.Salvage;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
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
/// Pre-builds cost maps at server startup for performance.
/// </summary>
public sealed class SalvageRuinGeneratorSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;
    private bool _attemptedPrebuild = false;

    /// <summary>
    /// Cached map data for each ruin map. Built at server startup.
    /// </summary>
    private readonly Dictionary<ResPath, CachedMapData> _cachedMapData = new();

    /// <summary>
    /// Cached data for a map, including cost map, coordinate map, wall entities, and window entities.
    /// </summary>
    private sealed class CachedMapData
    {
        public Dictionary<Vector2i, int> CostMap = new();
        public Dictionary<Vector2i, string> CoordinateMap = new();
        public List<(Vector2i Position, string PrototypeId)> WallEntities = new();
        public List<(Vector2i Position, string PrototypeId, Angle Rotation)> WindowEntities = new();
    }

    /// <summary>
    /// Result of ruin generation containing floor tiles to place, wall entities, and window entities to spawn.
    /// </summary>
    public sealed class RuinResult
    {
        public List<(Vector2i Position, Tile Tile)> FloorTiles = new();
        public List<(Vector2i Position, string PrototypeId)> WallEntities = new();
        public List<(Vector2i Position, string PrototypeId, Angle Rotation)> WindowEntities = new();
        public Box2 Bounds;
        
        // Configuration used for this ruin (needed for damage simulation when spawning)
        public SalvageMagnetRuinConfigPrototype? Config;
    }

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("system.salvage");
        
        // Subscribe to prototype reload events for hot reloading
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypesReloaded);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        
        // Try to prebuild cost maps on first tick when all systems are guaranteed ready
        if (!_attemptedPrebuild)
        {
            _attemptedPrebuild = true;
            
            var ruinMaps = _prototypeManager.EnumeratePrototypes<RuinMapPrototype>().ToList();
            if (ruinMaps.Count > 0)
            {
                _sawmill.Info("[SalvageRuinGenerator] Building cost maps for ruin maps on first tick...");
                PrebuildCostMaps();
            }
        }
    }

    private void OnPrototypesReloaded(PrototypesReloadedEventArgs args)
    {
        // Build cost maps when prototypes are first loaded, or rebuild if RuinMapPrototype was modified
        var isFirstLoad = _cachedMapData.Count == 0;
        var wasModified = args.WasModified<RuinMapPrototype>();
        
        if (isFirstLoad || wasModified)
        {
            if (isFirstLoad)
            {
                _sawmill.Info("[SalvageRuinGenerator] Prototypes loaded for first time, building cost maps for ruin maps...");
            }
            else
            {
                _sawmill.Info("[SalvageRuinGenerator] RuinMapPrototype prototypes were reloaded, rebuilding cost maps...");
                _cachedMapData.Clear();
            }
            PrebuildCostMaps();
        }
    }

    /// <summary>
    /// Pre-builds cost maps for all configured ruin maps at server startup.
    /// </summary>
    private void PrebuildCostMaps()
    {
        _sawmill.Info("[SalvageRuinGenerator] Pre-building cost maps for ruin maps...");
        
        var ruinMaps = _prototypeManager.EnumeratePrototypes<RuinMapPrototype>().ToList();
        
        // If no prototypes are loaded yet, skip pre-building (will build on-demand)
        if (ruinMaps.Count == 0)
        {
            _sawmill.Debug("[SalvageRuinGenerator] No RuinMapPrototype prototypes found yet, will build cost maps on-demand");
            return;
        }
        
        var successCount = 0;
        var failCount = 0;

        foreach (var ruinMap in ruinMaps)
        {
            if (BuildCostMapForMap(ruinMap.MapPath))
            {
                successCount++;
            }
            else
            {
                failCount++;
            }
        }

        _sawmill.Info($"[SalvageRuinGenerator] Pre-built cost maps: {successCount} succeeded, {failCount} failed");
    }

    /// <summary>
    /// Builds and caches cost map data for a specific map path.
    /// </summary>
    private bool BuildCostMapForMap(ResPath mapPath)
    {
        // Read YAML file directly without loading entities
        if (!_mapLoader.TryReadFile(mapPath, out var mapData))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Failed to read map file: {mapPath}");
            return false;
        }

        // Parse tilemap section
        var tileMap = new Dictionary<int, string>();
        if (!mapData.TryGet("tilemap", out MappingDataNode? tilemapNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing tilemap section");
            return false;
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
            return false;
        }

        // Parse grids section
        if (!mapData.TryGet("grids", out SequenceDataNode? gridsNode) || gridsNode.Sequence.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing or empty grids section");
            return false;
        }

        var firstGridUidNode = gridsNode.Sequence[0] as ValueDataNode;
        if (firstGridUidNode == null || !int.TryParse(firstGridUidNode.Value, out var firstGridUid))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} first grid UID is invalid");
            return false;
        }

        // Parse entities section
        if (!mapData.TryGet("entities", out SequenceDataNode? entitiesNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} missing entities section");
            return false;
        }

        // Find the grid entity
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
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} grid entity with UID {firstGridUid} not found");
            return false;
        }

        // Get MapGridComponent
        if (!gridEntityNode.TryGet("components", out SequenceDataNode? componentsNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} grid entity missing components section");
            return false;
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
            return false;
        }

        // Get chunks
        if (!mapGridComponent.TryGet("chunks", out MappingDataNode? chunksNode))
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} MapGrid component missing chunks section");
            return false;
        }

        // Get chunk size
        ushort chunkSize = 16;
        if (mapGridComponent.TryGet("chunksize", out ValueDataNode? chunkSizeNode))
        {
            if (ushort.TryParse(chunkSizeNode.Value, out var parsedChunkSize))
                chunkSize = parsedChunkSize;
        }

        // Build coordinate map from parsed chunks
        var coordinateMap = ParseChunks(chunksNode, tileMap, chunkSize);

        if (coordinateMap.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Map file {mapPath} produced empty coordinate map");
            return false;
        }

        // Parse wall entities
        var wallEntities = ParseWallEntities(entitiesNode, firstGridUid);

        // Parse window entities
        var windowEntities = ParseWindowEntities(entitiesNode, firstGridUid);

        // Get default config for cost map building (use "Default" if available, otherwise null for defaults)
        var defaultConfigId = new ProtoId<SalvageMagnetRuinConfigPrototype>("Default");
        SalvageMagnetRuinConfigPrototype? defaultConfig = null;
        _prototypeManager.TryIndex(defaultConfigId, out defaultConfig);

        // Build cost map (includes windows and walls in cost calculation)
        var costMap = BuildCostMap(coordinateMap, windowEntities, wallEntities, defaultConfig);

        // Cache the data
        _cachedMapData[mapPath] = new CachedMapData
        {
            CostMap = costMap,
            CoordinateMap = coordinateMap,
            WallEntities = wallEntities,
            WindowEntities = windowEntities
        };

        _sawmill.Debug($"[SalvageRuinGenerator] Cached cost map for {mapPath}: {costMap.Count} tiles, {wallEntities.Count} walls, {windowEntities.Count} windows");
        return true;
    }

    /// <summary>
    /// Generates a ruin from the specified map using flood-fill.
    /// Uses pre-built cost maps for performance.
    /// </summary>
    public RuinResult? GenerateRuin(ResPath mapPath, int seed, SalvageMagnetRuinConfigPrototype? config = null)
    {
        // Get cached map data
        if (!_cachedMapData.TryGetValue(mapPath, out var cachedData))
        {
            _sawmill.Info($"[SalvageRuinGenerator] Building cost map for {mapPath} on-demand...");
            // Build it on-demand (this is normal on first use if cache wasn't built at startup)
            if (!BuildCostMapForMap(mapPath) || !_cachedMapData.TryGetValue(mapPath, out cachedData))
            {
                _sawmill.Error($"[SalvageRuinGenerator] Failed to build cost map for {mapPath}");
                return null;
            }
            _sawmill.Info($"[SalvageRuinGenerator] Successfully built cost map for {mapPath} on-demand");
        }

        var costMap = cachedData.CostMap;
        var coordinateMap = cachedData.CoordinateMap;
        var wallEntities = cachedData.WallEntities;
        var windowEntities = cachedData.WindowEntities;

        // Get configuration values (defaults if not provided)
        var floodFillPoints = config?.FloodFillPoints ?? 50;
        var floodFillStages = config?.FloodFillStages ?? 5;
        var wallDestroyChance = config?.WallDestroyChance ?? 0.0f;
        var windowDamageChance = config?.WindowDamageChance ?? 0.0f;
        var floorToLatticeChance = config?.FloorToLatticeChance ?? 0.0f;
        var spaceCost = config?.SpaceCost ?? 99;
        var defaultTileCost = config?.DefaultTileCost ?? 1;

        // Find valid start location (retry up to 10 times)
        var rand = new System.Random(seed);
        var startPos = FindValidStartLocation(costMap, rand, maxRetries: 10, spaceCost);
        if (!startPos.HasValue)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Failed to find valid start location for map {mapPath} with seed {seed}");
            return null;
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Starting multi-stage flood-fill at {startPos.Value} with {floodFillStages} stages, {floodFillPoints} budget per stage");

        // Create sets of wall and window positions for fast lookup
        var wallPositions = new HashSet<Vector2i>(wallEntities.Select(w => w.Position));
        var windowPositions = new HashSet<Vector2i>(windowEntities.Select(w => w.Position));
        var allBlockingPositions = new HashSet<Vector2i>(wallPositions);
        allBlockingPositions.UnionWith(windowPositions);

        // Perform multi-stage flood-fill
        var region = FloodFillMultiStage(costMap, startPos.Value, floodFillStages, floodFillPoints, allBlockingPositions, rand, spaceCost, defaultTileCost);
        if (region.Count == 0)
        {
            _sawmill.Error($"[SalvageRuinGenerator] Flood-fill returned empty region for map {mapPath} with seed {seed}");
            return null;
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Flood-fill collected {region.Count} tiles");

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

        // Find window entities within or adjacent to the flood-filled region
        var windowEntitiesInRegion = new List<(Vector2i Position, string PrototypeId, Angle Rotation)>();
        
        foreach (var (windowPos, windowProto, windowRotation) in windowEntities)
        {
            // Check if window is within the region or adjacent to it
            if (regionSet.Contains(windowPos))
            {
                // Window is within the flood-filled region
                windowEntitiesInRegion.Add((windowPos, windowProto, windowRotation));
                // Update bounds to include windows
                minX = Math.Min(minX, windowPos.X);
                minY = Math.Min(minY, windowPos.Y);
                maxX = Math.Max(maxX, windowPos.X);
                maxY = Math.Max(maxY, windowPos.Y);
            }
            else
            {
                // Check if window is adjacent to the region
                var neighbors = new[]
                {
                    new Vector2i(windowPos.X + 1, windowPos.Y),
                    new Vector2i(windowPos.X - 1, windowPos.Y),
                    new Vector2i(windowPos.X, windowPos.Y + 1),
                    new Vector2i(windowPos.X, windowPos.Y - 1)
                };

                foreach (var neighbor in neighbors)
                {
                    if (regionSet.Contains(neighbor))
                    {
                        windowEntitiesInRegion.Add((windowPos, windowProto, windowRotation));
                        // Update bounds to include windows
                        minX = Math.Min(minX, windowPos.X);
                        minY = Math.Min(minY, windowPos.Y);
                        maxX = Math.Max(maxX, windowPos.X);
                        maxY = Math.Max(maxY, windowPos.Y);
                        break; // Only add once
                    }
                }
            }
        }

        _sawmill.Debug($"[SalvageRuinGenerator] Found {windowEntitiesInRegion.Count} window entities in or adjacent to region");

        // Now normalize all coordinates to start from (0,0) relative to the ruin's origin
        var originX = minX;
        var originY = minY;

        // Create random instance for damage simulation
        var damageRand = new System.Random(seed);

        // Add all tiles from the flood-filled region (floors) with normalized coordinates
        // Apply floor-to-lattice damage simulation IN MEMORY before spawning
        foreach (var pos in region)
        {
            if (!coordinateMap.TryGetValue(pos, out var tileId))
                continue;

            // Get tile definition
            if (!_prototypeManager.TryIndex<ContentTileDefinition>(tileId, out var tileDef))
                continue;

            // Check if this tile is already lattice (never damage lattice further)
            var isLattice = tileDef.ID.Equals("Lattice", StringComparison.OrdinalIgnoreCase);

            Tile tile;
            if (!isLattice && floorToLatticeChance > 0.0f && damageRand.NextSingle() < floorToLatticeChance)
            {
                // Replace floor tile with lattice
                if (!_tileDefinitionManager.TryGetDefinition("Lattice", out var latticeDef))
                {
                    // Fallback to original tile if lattice not found
                    tile = new Tile(tileDef.TileId);
                }
                else
                {
                    tile = new Tile(latticeDef.TileId);
                }
            }
            else
            {
                // Keep original tile
                tile = new Tile(tileDef.TileId);
            }

            // Normalize coordinates relative to ruin origin
            var normalizedPos = new Vector2i(pos.X - originX, pos.Y - originY);
            tilesToPlace[normalizedPos] = tile;
        }

        // Add adjacent wall entities with normalized coordinates
        // Apply wall destruction simulation IN MEMORY before spawning
        foreach (var (wallPos, wallProto) in adjacentWallEntities)
        {
            // Check if wall should be destroyed
            if (wallDestroyChance > 0.0f && damageRand.NextSingle() < wallDestroyChance)
            {
                // Wall is destroyed, skip it (don't add to result)
                continue;
            }

            var normalizedWallPos = new Vector2i(wallPos.X - originX, wallPos.Y - originY);
            result.WallEntities.Add((normalizedWallPos, wallProto));
        }

        // Add window entities with normalized coordinates and preserved rotation
        // Window damage will be applied when spawning, but we track which ones should be damaged
        foreach (var (windowPos, windowProto, windowRotation) in windowEntitiesInRegion)
        {
            var normalizedWindowPos = new Vector2i(windowPos.X - originX, windowPos.Y - originY);
            result.WindowEntities.Add((normalizedWindowPos, windowProto, windowRotation));
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

        _sawmill.Debug($"[SalvageRuinGenerator] Generated ruin with {tilesToPlace.Count} tiles ({region.Count} floors), {result.WallEntities.Count} wall entities (after destruction), and {windowEntitiesInRegion.Count} window entities");

        // Convert to list
        result.FloorTiles = tilesToPlace.Select(kvp => (kvp.Key, kvp.Value)).ToList();

        // Calculate bounds (add 1 for inclusive bounds)
        result.Bounds = new Box2(normalizedMinX, normalizedMinY, normalizedMaxX + 1, normalizedMaxY + 1);

        // Store config for damage simulation when spawning
        result.Config = config;

        return result;
    }

    /// <summary>
    /// Parses chunks from YAML data and builds a coordinate map of tile positions to tile IDs.
    /// </summary>
    private Dictionary<Vector2i, string> ParseChunks(MappingDataNode chunksNode, Dictionary<int, string> tileMap, ushort chunkSize)
    {
        var coordinateMap = new Dictionary<Vector2i, string>();

        _sawmill.Debug($"[SalvageRuinGenerator] ParseChunks: Processing {chunksNode.Children.Count} chunks with chunk size {chunkSize}");

        foreach (var (chunkIndexKey, chunkValueNode) in chunksNode.Children)
        {
            var chunkIndexStr = chunkIndexKey;
            var chunkIndexParts = chunkIndexStr.Split(',');
            if (chunkIndexParts.Length != 2 ||
                !int.TryParse(chunkIndexParts[0], out var chunkX) ||
                !int.TryParse(chunkIndexParts[1], out var chunkY))
            {
                _sawmill.Warning($"[SalvageRuinGenerator] Invalid chunk index format: {chunkIndexStr}");
                continue;
            }

            if (chunkValueNode is not MappingDataNode chunkNode)
            {
                _sawmill.Warning($"[SalvageRuinGenerator] Chunk value is not a mapping node for chunk {chunkIndexStr}");
                continue;
            }

            if (!chunkNode.TryGet("tiles", out ValueDataNode? tilesNode))
            {
                _sawmill.Warning($"[SalvageRuinGenerator] Chunk {chunkIndexStr} missing 'tiles' data");
                continue;
            }

            int version = 7;
            if (chunkNode.TryGet("version", out ValueDataNode? versionNode))
                int.TryParse(versionNode.Value, out version);

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

            var tilesAddedInChunk = 0;
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

                    if (!tileMap.TryGetValue(yamlTileId, out var tileDefName))
                        continue;

                    if (!_tileDefinitionManager.TryGetDefinition(tileDefName, out var tileDef))
                        continue;

                    var worldX = chunkX * chunkSize + x;
                    var worldY = chunkY * chunkSize + y;
                    var worldPos = new Vector2i(worldX, worldY);

                    if (tileDef.TileId != 0)
                    {
                        coordinateMap[worldPos] = tileDef.ID;
                        tilesAddedInChunk++;
                    }
                }
            }
            
            if (tilesAddedInChunk == 0)
                _sawmill.Warning($"[SalvageRuinGenerator] Chunk {chunkX},{chunkY} produced no valid tiles");
        }

        _sawmill.Debug($"[SalvageRuinGenerator] ParseChunks: Produced {coordinateMap.Count} tiles total");
        return coordinateMap;
    }

    /// <summary>
    /// Parses wall entities from the map file and returns their positions and prototype IDs.
    /// </summary>
    private List<(Vector2i Position, string PrototypeId)> ParseWallEntities(SequenceDataNode entitiesNode, int gridUid)
    {
        var wallEntities = new List<(Vector2i, string)>();

        var wallPrototypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "WallSolid",
            "WallReinforced",
            "WallReinforcedRust",
            "WallSolidRust",
            // Diagonal walls
            "WallSolidDiagonal",
            "WallReinforcedDiagonal",
            "WallShuttleDiagonal",
            "WallPlastitaniumDiagonal",
            "WallMiningDiagonal",
        };

        foreach (var protoGroupNode in entitiesNode.Sequence.Cast<MappingDataNode>())
        {
            if (!protoGroupNode.TryGet("proto", out ValueDataNode? protoNode))
                continue;

            var protoId = protoNode.Value;
            
            if (!wallPrototypes.Contains(protoId))
                continue;

            if (!protoGroupNode.TryGet("entities", out SequenceDataNode? entitiesInGroup))
                continue;

            foreach (var entityNode in entitiesInGroup.Sequence.Cast<MappingDataNode>())
            {
                if (entityNode.TryGet("uid", out ValueDataNode? uidNode) &&
                    int.TryParse(uidNode.Value, out var entityUid) &&
                    entityUid == gridUid)
                    continue;

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
                        if (componentNode.TryGet("pos", out ValueDataNode? posNode))
                        {
                            var posParts = posNode.Value.Split(',');
                            if (posParts.Length == 2 &&
                                float.TryParse(posParts[0], out var x) &&
                                float.TryParse(posParts[1], out var y))
                            {
                                entityPos = new Vector2i((int)Math.Floor(x), (int)Math.Floor(y));
                            }
                        }

                        if (componentNode.TryGet("parent", out ValueDataNode? parentNode) &&
                            int.TryParse(parentNode.Value, out var parent))
                        {
                            parentUid = parent;
                        }
                    }
                }

                if (entityPos.HasValue && parentUid == gridUid)
                {
                    wallEntities.Add((entityPos.Value, protoId));
                }
            }
        }

        return wallEntities;
    }

    /// <summary>
    /// Parses window entities from the map file and returns their positions, prototype IDs, and rotations.
    /// </summary>
    private List<(Vector2i Position, string PrototypeId, Angle Rotation)> ParseWindowEntities(SequenceDataNode entitiesNode, int gridUid)
    {
        var windowEntities = new List<(Vector2i, string, Angle)>();

        foreach (var protoGroupNode in entitiesNode.Sequence.Cast<MappingDataNode>())
        {
            if (!protoGroupNode.TryGet("proto", out ValueDataNode? protoNode))
                continue;

            var protoId = protoNode.Value;
            
            // Check if this is a window entity (contains "Window" in the ID)
            if (!IsWindowEntity(protoId))
                continue;

            if (!protoGroupNode.TryGet("entities", out SequenceDataNode? entitiesInGroup))
                continue;

            foreach (var entityNode in entitiesInGroup.Sequence.Cast<MappingDataNode>())
            {
                if (entityNode.TryGet("uid", out ValueDataNode? uidNode) &&
                    int.TryParse(uidNode.Value, out var entityUid) &&
                    entityUid == gridUid)
                    continue;

                if (!entityNode.TryGet("components", out SequenceDataNode? componentsNode))
                    continue;

                Vector2i? entityPos = null;
                int? parentUid = null;
                Angle rotation = Angle.Zero;

                foreach (var componentNode in componentsNode.Sequence.Cast<MappingDataNode>())
                {
                    if (!componentNode.TryGet("type", out ValueDataNode? typeNode))
                        continue;

                    if (typeNode.Value == "Transform")
                    {
                        if (componentNode.TryGet("pos", out ValueDataNode? posNode))
                        {
                            var posParts = posNode.Value.Split(',');
                            if (posParts.Length == 2 &&
                                float.TryParse(posParts[0], out var x) &&
                                float.TryParse(posParts[1], out var y))
                            {
                                entityPos = new Vector2i((int)Math.Floor(x), (int)Math.Floor(y));
                            }
                        }

                        if (componentNode.TryGet("parent", out ValueDataNode? parentNode) &&
                            int.TryParse(parentNode.Value, out var parent))
                        {
                            parentUid = parent;
                        }

                        // Parse rotation from "rot" field (stored as "X rad" or just "X")
                        if (componentNode.TryGet("rot", out ValueDataNode? rotNode))
                        {
                            var rotStr = rotNode.Value.Trim();
                            // Remove "rad" suffix if present
                            if (rotStr.EndsWith("rad", StringComparison.OrdinalIgnoreCase))
                            {
                                rotStr = rotStr.Substring(0, rotStr.Length - 3).Trim();
                            }
                            
                            if (float.TryParse(rotStr, out var rotValue))
                            {
                                rotation = new Angle(rotValue);
                            }
                        }
                    }
                }

                if (entityPos.HasValue && parentUid == gridUid)
                {
                    windowEntities.Add((entityPos.Value, protoId, rotation));
                }
            }
        }

        return windowEntities;
    }

    /// <summary>
    /// Checks if a prototype ID represents a window entity.
    /// </summary>
    private bool IsWindowEntity(string prototypeId)
    {
        // Check for common window patterns
        var id = prototypeId.ToLowerInvariant();
        
        // Include windows, windoors, and firelocks
        return (id.Contains("window", StringComparison.OrdinalIgnoreCase) ||
                id.Contains("windoor", StringComparison.OrdinalIgnoreCase) ||
                id.Contains("firelock", StringComparison.OrdinalIgnoreCase)) &&
               !id.Contains("frame", StringComparison.OrdinalIgnoreCase) && // Exclude frames/assemblies
               !id.Contains("assembly", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Builds a cost map from coordinate map. Space tiles (missing from map) get cost from config (default 99).
    /// Window and wall entities at positions get appropriate cost based on type.
    /// </summary>
    private Dictionary<Vector2i, int> BuildCostMap(Dictionary<Vector2i, string> coordinateMap, List<(Vector2i Position, string PrototypeId, Angle Rotation)> windowEntities, List<(Vector2i Position, string PrototypeId)> wallEntities, SalvageMagnetRuinConfigPrototype? config = null)
    {
        var costMap = new Dictionary<Vector2i, int>();
        
        // Create sets of window and wall positions for fast lookup
        var windowPositions = new HashSet<Vector2i>(windowEntities.Select(w => w.Position));
        var wallPositions = new HashSet<Vector2i>(wallEntities.Select(w => w.Position));

        // Get wall cost from config
        var wallCost = config?.WallCost ?? 6;

        foreach (var (pos, tileId) in coordinateMap)
        {
            // Priority: walls > windows > tiles
            // If there's a wall entity at this position, use wall cost
            if (wallPositions.Contains(pos))
            {
                costMap[pos] = wallCost;
            }
            // If there's a window entity at this position, use window cost
            else if (windowPositions.Contains(pos))
            {
                var windowEntity = windowEntities.FirstOrDefault(w => w.Position == pos);
                if (windowEntity.PrototypeId != null)
                {
                    var cost = GetWindowCost(windowEntity.PrototypeId, config);
                    costMap[pos] = cost;
                }
                else
                {
                    // Fallback to tile cost if window not found
                    var cost = GetTileCost(tileId, config);
                    costMap[pos] = cost;
                }
            }
            else
            {
                var cost = GetTileCost(tileId, config);
                costMap[pos] = cost;
            }
        }

        // Also add wall positions that might not have floor tiles underneath
        foreach (var (wallPos, _) in wallEntities)
        {
            if (!costMap.ContainsKey(wallPos))
            {
                costMap[wallPos] = wallCost;
            }
        }

        // Also add window positions that might not have floor tiles underneath
        foreach (var (windowPos, windowProto, _) in windowEntities)
        {
            if (!costMap.ContainsKey(windowPos))
            {
                var cost = GetWindowCost(windowProto, config);
                costMap[windowPos] = cost;
            }
        }

        return costMap;
    }

    /// <summary>
    /// Gets the cost for a window entity based on its prototype ID.
    /// </summary>
    private int GetWindowCost(string prototypeId, SalvageMagnetRuinConfigPrototype? config = null)
    {
        var id = prototypeId.ToLowerInvariant();
        
        // Firelocks (treated as reinforced barriers)
        if (id.Contains("firelock", StringComparison.OrdinalIgnoreCase))
        {
            return config?.ReinforcedWindowCost ?? 4;
        }
        
        // Windoors (secure windoors are reinforced, regular windoors are not)
        if (id.Contains("windoor", StringComparison.OrdinalIgnoreCase))
        {
            if (id.Contains("secure", StringComparison.OrdinalIgnoreCase))
                return config?.ReinforcedWindowCost ?? 4;
            else
                return config?.RegularWindowCost ?? 2;
        }
        
        // Directional Reinforced windows (most expensive)
        if (id.Contains("directional", StringComparison.OrdinalIgnoreCase) && 
            id.Contains("reinforced", StringComparison.OrdinalIgnoreCase))
        {
            return config?.ReinforcedWindowCost ?? 4;
        }
        
        // Directional windows (regular)
        if (id.Contains("directional", StringComparison.OrdinalIgnoreCase))
        {
            return config?.DirectionalWindowCost ?? 2;
        }
        
        // Diagonal windows (treated similar to directional)
        if (id.Contains("diagonal", StringComparison.OrdinalIgnoreCase))
        {
            if (id.Contains("reinforced", StringComparison.OrdinalIgnoreCase))
                return config?.ReinforcedWindowCost ?? 4;
            else
                return config?.DirectionalWindowCost ?? 2;
        }
        
        // Reinforced windows (non-directional)
        if (id.Contains("reinforced", StringComparison.OrdinalIgnoreCase))
        {
            return config?.ReinforcedWindowCost ?? 4;
        }
        
        // Regular windows
        return config?.RegularWindowCost ?? 2;
    }

    /// <summary>
    /// Checks if a tile is a wall based on its prototype ID.
    /// </summary>
    private bool IsWallTile(string tileId)
    {
        return tileId.Equals("WallSolid", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallReinforced", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallReinforcedRust", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallSolidRust", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallSolidDiagonal", StringComparison.OrdinalIgnoreCase) ||
               tileId.Equals("WallReinforcedDiagonal", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the cost for a tile based on its prototype ID.
    /// </summary>
    private int GetTileCost(string tileId, SalvageMagnetRuinConfigPrototype? config = null)
    {
        if (IsWallTile(tileId))
            return config?.WallCost ?? 6;

        if (tileId.Contains("DirectionalGlass", StringComparison.OrdinalIgnoreCase))
            return config?.DirectionalGlassCost ?? 2;

        if (tileId.Contains("ReinforcedGlass", StringComparison.OrdinalIgnoreCase))
            return config?.ReinforcedGlassCost ?? 4;

        if (tileId.Contains("Glass", StringComparison.OrdinalIgnoreCase) &&
            !tileId.Contains("Directional", StringComparison.OrdinalIgnoreCase))
            return config?.RegularGlassCost ?? 4;

        if (tileId.Contains("Grille", StringComparison.OrdinalIgnoreCase))
            return config?.GrilleCost ?? 2;

        return config?.DefaultTileCost ?? 1;
    }

    /// <summary>
    /// Finds a valid start location (non-space tile) with retries.
    /// </summary>
    private Vector2i? FindValidStartLocation(Dictionary<Vector2i, int> costMap, System.Random rand, int maxRetries, int spaceCost = 99)
    {
        if (costMap.Count == 0)
            return null;

        var positions = costMap.Keys.ToList();

        for (var i = 0; i < maxRetries; i++)
        {
            var pos = positions[rand.Next(positions.Count)];
            var cost = costMap[pos];

            if (cost < spaceCost)
                return pos;
        }

        foreach (var (pos, cost) in costMap)
        {
            if (cost < spaceCost)
                return pos;
        }

        return null;
    }

    /// <summary>
    /// Performs cost-based flood-fill from start position, collecting tiles until budget cost is exhausted.
    /// Uses accumulated cost to determine when to stop, not tile count.
    /// When budget is exhausted, extends the result to include adjacent tiles with walls.
    /// </summary>
    private HashSet<Vector2i> FloodFillWithCost(Dictionary<Vector2i, int> costMap, Vector2i start, int budget, HashSet<Vector2i> wallPositions)
    {
        var result = new HashSet<Vector2i>();
        var visited = new HashSet<Vector2i>();
        var queue = new List<(Vector2i Pos, int AccumulatedCost)>();
        var accumulatedCosts = new Dictionary<Vector2i, int>();
        var totalCostSpent = 0; // Track total cost spent, not tile count

        queue.Add((start, 0));
        accumulatedCosts[start] = 0;

        // CRITICAL FIX: Stop when accumulated cost exceeds budget, not when tile count exceeds budget
        while (queue.Count > 0)
        {
            queue.Sort((a, b) => a.AccumulatedCost.CompareTo(b.AccumulatedCost));
            var (current, accumulatedCost) = queue[0];
            queue.RemoveAt(0);

            if (visited.Contains(current))
                continue;

            visited.Add(current);

            if (!costMap.TryGetValue(current, out var tileCost))
                continue;

            // Check if adding this tile would exceed the budget
            // accumulatedCost is the cost to reach this tile (path cost), 
            // but we need to pay the tile's cost to add it to the result
            // So total cost = accumulatedCost (path) + tileCost (tile itself)
            var totalCostIfAdded = accumulatedCost + tileCost;
            if (totalCostIfAdded > budget)
            {
                // Can't afford this tile, stop the flood-fill
                break;
            }

            // Add the tile to result and update total cost spent
            result.Add(current);
            totalCostSpent = totalCostIfAdded;

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

                if (!costMap.TryGetValue(neighbor, out var neighborCost))
                    continue;

                var newAccumulatedCost = accumulatedCost + neighborCost;

                // Only consider neighbors we can afford
                if (newAccumulatedCost > budget)
                    continue;

                if (!accumulatedCosts.TryGetValue(neighbor, out var existingCost) ||
                    newAccumulatedCost < existingCost)
                {
                    accumulatedCosts[neighbor] = newAccumulatedCost;
                    queue.Add((neighbor, newAccumulatedCost));
                }
            }
        }

        // If we stopped due to budget exhaustion, check adjacent tiles for walls
        // and add them to ensure the ruin has proper boundaries
        if (result.Count >= budget)
        {
            var adjacentWithWalls = new HashSet<Vector2i>();
            
            // Find all tiles adjacent to the flood-filled region
            foreach (var pos in result)
            {
                var neighbors = new[]
                {
                    pos + Vector2i.Up,
                    pos + Vector2i.Down,
                    pos + Vector2i.Left,
                    pos + Vector2i.Right
                };

                foreach (var neighbor in neighbors)
                {
                    // Skip if already in result
                    if (result.Contains(neighbor))
                        continue;

                    // Check if this adjacent tile has a wall entity
                    if (wallPositions.Contains(neighbor))
                    {
                        adjacentWithWalls.Add(neighbor);
                    }
                }
            }

            // Add all adjacent wall tiles to the result
            foreach (var wallPos in adjacentWithWalls)
            {
                result.Add(wallPos);
            }

            if (adjacentWithWalls.Count > 0)
            {
                _sawmill.Debug($"[SalvageRuinGenerator] Extended flood-fill to include {adjacentWithWalls.Count} adjacent wall tiles");
            }
        }

        return result;
    }

    /// <summary>
    /// Performs multi-stage cost-based flood-fill, creating irregular branching shapes.
    /// Each stage starts from a random tile adjacent to the previous stages' results,
    /// ensuring connectivity while creating organic, branching ruin structures.
    /// Prioritizes low-cost tiles (floors) for stage starting positions.
    /// </summary>
    private HashSet<Vector2i> FloodFillMultiStage(Dictionary<Vector2i, int> costMap, Vector2i start, int stagesCount, int budgetPerStage, HashSet<Vector2i> wallPositions, System.Random rand, int spaceCost, int defaultTileCost)
    {
        var result = new HashSet<Vector2i>();
        var visited = new HashSet<Vector2i>();
        var currentStart = start;

        for (var stage = 0; stage < stagesCount; stage++)
        {
            var stageResult = new HashSet<Vector2i>();
            var stageVisited = new HashSet<Vector2i>();
            var queue = new List<(Vector2i Pos, int AccumulatedCost)>();
            var accumulatedCosts = new Dictionary<Vector2i, int>();

            queue.Add((currentStart, 0));
            accumulatedCosts[currentStart] = 0;

            // Perform flood-fill for this stage
            while (queue.Count > 0)
            {
                queue.Sort((a, b) => a.AccumulatedCost.CompareTo(b.AccumulatedCost));
                var (current, accumulatedCost) = queue[0];
                queue.RemoveAt(0);

                if (stageVisited.Contains(current))
                    continue;

                stageVisited.Add(current);

                if (!costMap.TryGetValue(current, out var tileCost))
                    continue;

                // Check if adding this tile would exceed the budget
                var totalCostIfAdded = accumulatedCost + tileCost;
                if (totalCostIfAdded > budgetPerStage)
                {
                    // Can't afford this tile, skip it
                    continue;
                }

                // Add the tile to stage result
                stageResult.Add(current);
                result.Add(current);
                visited.Add(current);

                var neighbors = new[]
                {
                    current + Vector2i.Up,
                    current + Vector2i.Down,
                    current + Vector2i.Left,
                    current + Vector2i.Right
                };

                foreach (var neighbor in neighbors)
                {
                    if (stageVisited.Contains(neighbor) || visited.Contains(neighbor))
                        continue;

                    if (!costMap.TryGetValue(neighbor, out var neighborCost))
                        continue;

                    var newAccumulatedCost = accumulatedCost + neighborCost;

                    // Only consider neighbors we can afford
                    if (newAccumulatedCost > budgetPerStage)
                        continue;

                    if (!accumulatedCosts.TryGetValue(neighbor, out var existingCost) ||
                        newAccumulatedCost < existingCost)
                    {
                        accumulatedCosts[neighbor] = newAccumulatedCost;
                        queue.Add((neighbor, newAccumulatedCost));
                    }
                }
            }

            // Extend stage result to include adjacent wall tiles (same as single-stage)
            var adjacentWithWalls = new HashSet<Vector2i>();
            foreach (var pos in stageResult)
            {
                var neighbors = new[]
                {
                    pos + Vector2i.Up,
                    pos + Vector2i.Down,
                    pos + Vector2i.Left,
                    pos + Vector2i.Right
                };

                foreach (var neighbor in neighbors)
                {
                    if (result.Contains(neighbor))
                        continue;

                    if (wallPositions.Contains(neighbor))
                    {
                        adjacentWithWalls.Add(neighbor);
                    }
                }
            }

            foreach (var wallPos in adjacentWithWalls)
            {
                result.Add(wallPos);
                visited.Add(wallPos);
            }

            _sawmill.Debug($"[SalvageRuinGenerator] Stage {stage + 1}/{stagesCount}: Added {stageResult.Count} tiles, {adjacentWithWalls.Count} adjacent walls");

            // Pick next start position from tiles directly adjacent to current result
            // Prioritize low-cost tiles (floors) over high-cost tiles (walls/windows) for better expansion
            if (stage < stagesCount - 1)
            {
                
                // Build lists of adjacent unvisited tiles, grouped by cost priority
                var lowCostTiles = new List<Vector2i>();      // defaultTileCost tiles (floors)
                var mediumCostTiles = new List<Vector2i>();   // Medium cost tiles (windows, grilles)
                var highCostTiles = new List<Vector2i>();     // High cost tiles (walls)
                
                foreach (var pos in result)
                {
                    var neighbors = new[]
                    {
                        pos + Vector2i.Up,
                        pos + Vector2i.Down,
                        pos + Vector2i.Left,
                        pos + Vector2i.Right
                    };

                    foreach (var neighbor in neighbors)
                    {
                        // Skip if already visited or in result
                        if (visited.Contains(neighbor) || result.Contains(neighbor))
                            continue;

                        // Check if this is a valid tile (not space)
                        if (!costMap.TryGetValue(neighbor, out var neighborCost))
                            continue;

                        if (neighborCost >= spaceCost)
                            continue;

                        // Group by cost priority
                        if (neighborCost <= defaultTileCost)
                        {
                            if (!lowCostTiles.Contains(neighbor))
                                lowCostTiles.Add(neighbor);
                        }
                        else if (neighborCost <= 5)
                        {
                            if (!mediumCostTiles.Contains(neighbor))
                                mediumCostTiles.Add(neighbor);
                        }
                        else
                        {
                            if (!highCostTiles.Contains(neighbor))
                                highCostTiles.Add(neighbor);
                        }
                    }
                }

                // Select from priority groups: low cost first, then medium, then high
                if (lowCostTiles.Count > 0)
                {
                    currentStart = lowCostTiles[rand.Next(lowCostTiles.Count)];
                    _sawmill.Debug($"[SalvageRuinGenerator] Stage {stage + 1} complete, starting stage {stage + 2} from low-cost tile {currentStart} ({lowCostTiles.Count} low-cost candidates)");
                }
                else if (mediumCostTiles.Count > 0)
                {
                    currentStart = mediumCostTiles[rand.Next(mediumCostTiles.Count)];
                    _sawmill.Debug($"[SalvageRuinGenerator] Stage {stage + 1} complete, starting stage {stage + 2} from medium-cost tile {currentStart} ({mediumCostTiles.Count} medium-cost candidates)");
                }
                else if (highCostTiles.Count > 0)
                {
                    currentStart = highCostTiles[rand.Next(highCostTiles.Count)];
                    _sawmill.Debug($"[SalvageRuinGenerator] Stage {stage + 1} complete, starting stage {stage + 2} from high-cost tile {currentStart} ({highCostTiles.Count} high-cost candidates)");
                }
                else
                {
                    // No adjacent tiles available, stop early (map exhausted)
                    _sawmill.Debug($"[SalvageRuinGenerator] Stage {stage + 1} complete, no adjacent unvisited tiles available (map exhausted), stopping early after {stage + 1} stages");
                    break;
                }
            }
        }

        return result;
    }
}
