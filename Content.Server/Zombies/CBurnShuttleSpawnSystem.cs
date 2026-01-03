// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Numerics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Humanoid.Systems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Zombies;
using Robust.Server.GameObjects;
using Robust.Server.Player;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Zombies;

/// <summary>
/// System that monitors zombie infection percentage and spawns CBURN shuttles when 55% of players are dead/zombified.
/// </summary>
public sealed partial class CBurnShuttleSpawnSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly RandomHumanoidSystem _randomHumanoid = default!;

    private const int PlayersPerShuttle = 10;
    private static readonly ResPath ShuttlePath = new("/Maps/_Funkystation/Shuttles/ERTShuttles/ERTBurnWagonV2.yml");

    public override void Initialize()
    {
        base.Initialize();
        InitializeCommands();
    }

    // No Update loop needed - spawning is triggered by ZombieRuleSystem

    /// <summary>
    /// Counts total players and calculates number of shuttles needed (1 per 10 players).
    /// </summary>
    private int CalculateShuttleCount()
    {
        var playerCount = 0;
        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity != null)
                playerCount++;
        }

        // 1 shuttle per 10 players, minimum 1
        return Math.Max(1, (int)Math.Ceiling(playerCount / (float)PlayersPerShuttle));
    }

    /// <summary>
    /// Parses the ERTBurnWagonV2.yml file to extract paper positions.
    /// </summary>
    private List<Vector2> ParsePaperPositions()
    {
        var paperPositions = new List<Vector2>();

        if (!_mapLoader.TryReadFile(ShuttlePath, out var data))
        {
            Log.Error($"Failed to read CBURN shuttle file: {ShuttlePath}");
            return paperPositions;
        }

        if (!data.TryGet("entities", out SequenceDataNode? entitiesNode))
        {
            Log.Error($"CBURN shuttle file missing entities section: {ShuttlePath}");
            return paperPositions;
        }

        // Find the grid UID (first grid in grids list)
        int? gridUid = null;
        if (data.TryGet("grids", out SequenceDataNode? gridsNode) && gridsNode.Sequence.Count > 0)
        {
            if (gridsNode.Sequence[0] is ValueDataNode gridUidNode)
            {
                if (int.TryParse(gridUidNode.Value, out var gridId))
                {
                    gridUid = gridId;
                }
            }
        }

        // Traverse entities to find PaperNanoTaskItem
        foreach (var protoGroupNode in entitiesNode.Sequence.Cast<MappingDataNode>())
        {
            if (!protoGroupNode.TryGet("proto", out ValueDataNode? protoNode))
                continue;

            var protoId = protoNode.Value;
            if (protoId != "PaperNanoTaskItem")
                continue;

            if (!protoGroupNode.TryGet("entities", out SequenceDataNode? entitiesInGroup))
                continue;

            foreach (var entityNode in entitiesInGroup.Sequence.Cast<MappingDataNode>())
            {
                if (!entityNode.TryGet("components", out SequenceDataNode? componentsNode))
                    continue;

                Vector2? entityPos = null;
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
                                float.TryParse(posParts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var x) &&
                                float.TryParse(posParts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var y))
                            {
                                entityPos = new Vector2(x, y);
                            }
                        }

                        if (componentNode.TryGet("parent", out ValueDataNode? parentNode))
                        {
                            if (parentNode.Value == "invalid")
                                parentUid = null;
                            else if (int.TryParse(parentNode.Value, out var parent))
                                parentUid = parent;
                        }
                    }
                }

                // Only add positions that are children of the grid
                // If gridUid is null, we can't verify parent, so skip
                // Otherwise, only add if parent matches grid or parent is invalid (meaning it's a direct child)
                if (entityPos.HasValue && gridUid.HasValue)
                {
                    // Add if parent matches grid UID, or if parent is invalid/null (direct child of grid)
                    if (parentUid == gridUid || !parentUid.HasValue)
                    {
                        paperPositions.Add(entityPos.Value);
                    }
                }
            }
        }

        return paperPositions;
    }

    /// <summary>
    /// Spawns CBURN shuttles and adds ghost roles at paper positions.
    /// Called by ZombieRuleSystem when 55% threshold is reached.
    /// </summary>
    /// <param name="spawnComp">The spawn component to track spawned shuttles</param>
    /// <param name="shuttleCount">Optional count. If null, auto-calculates based on player count.</param>
    /// <param name="maxGhostRoles">Optional maximum number of ghost roles to spawn. If null, spawns roles for all paper positions.</param>
    public void SpawnCBurnShuttles(CBurnShuttleSpawnComponent spawnComp, int? shuttleCount = null, int? maxGhostRoles = null)
    {
        var paperPositions = ParsePaperPositions();

        if (paperPositions.Count == 0)
        {
            Log.Warning("No paper positions found in CBURN shuttle file, cannot spawn ghost roles");
            return;
        }

        int count;
        if (maxGhostRoles.HasValue)
        {
            // Calculate number of shuttles needed to accommodate the desired number of ghost roles
            count = Math.Max(1, (int)Math.Ceiling(maxGhostRoles.Value / (float)paperPositions.Count));
        }
        else
        {
            // Use shuttle count (default: auto-calculate based on player count)
            count = shuttleCount ?? CalculateShuttleCount();
        }

        Log.Info($"Spawning {count} CBURN shuttle(s) with {paperPositions.Count} paper position(s) per shuttle{(maxGhostRoles.HasValue ? $" (targeting {maxGhostRoles.Value} ghost roles)" : "")}");

        var usedPositions = new HashSet<Vector2>(); // Track used positions across all shuttles
        var totalGhostRolesAdded = 0;

        for (int i = 0; i < count; i++)
        {
            // Create new map for this shuttle
            _mapSystem.CreateMap(out var mapId);
            var opts = DeserializationOptions.Default with { InitializeMaps = true };

            if (!_mapLoader.TryLoadGrid(mapId, ShuttlePath, out var grid, opts))
            {
                Log.Error($"Failed to load CBURN shuttle grid {i + 1}/{count}");
                continue;
            }

            var shuttleUid = grid.Value.Owner;
            spawnComp.SpawnedShuttles.Add(shuttleUid);

            Log.Info($"Spawned CBURN shuttle {i + 1}/{count} at {ToPrettyString(shuttleUid)}");

            // Calculate how many ghost roles we still need
            int? remainingRoles = maxGhostRoles.HasValue ? maxGhostRoles.Value - totalGhostRolesAdded : null;
            
            // Add ghost roles at paper positions
            var added = AddGhostRolesToShuttle(shuttleUid, paperPositions, usedPositions, remainingRoles);
            totalGhostRolesAdded += added;

            // Stop if we've reached the target number of ghost roles
            if (maxGhostRoles.HasValue && totalGhostRolesAdded >= maxGhostRoles.Value)
                break;
        }

        Log.Info($"Spawned {totalGhostRolesAdded} CBURN ghost role(s) across {spawnComp.SpawnedShuttles.Count} shuttle(s)");
    }

    /// <summary>
    /// Spawns CBURN agent entities at paper positions in the spawned shuttle.
    /// </summary>
    /// <param name="shuttleUid">The shuttle entity to spawn agents in</param>
    /// <param name="paperPositions">List of paper positions from the YAML file</param>
    /// <param name="usedPositions">Set of positions already used (to prevent duplicates across shuttles)</param>
    /// <param name="maxRoles">Maximum number of agents to spawn on this shuttle. If null, spawns agents at all available positions.</param>
    /// <returns>Number of agents actually spawned</returns>
    private int AddGhostRolesToShuttle(EntityUid shuttleUid, List<Vector2> paperPositions, HashSet<Vector2> usedPositions, int? maxRoles = null)
    {
        var shuttleGrid = shuttleUid;
        var addedCount = 0;

        // Match paper positions and spawn agents
        foreach (var paperPos in paperPositions)
        {
            // Stop if we've reached the maximum for this shuttle
            if (maxRoles.HasValue && addedCount >= maxRoles.Value)
                break;

            // Skip if this position was already used
            if (usedPositions.Contains(paperPos))
                continue;

            // Convert local grid position to EntityCoordinates
            var localCoords = new EntityCoordinates(shuttleGrid, paperPos);
            
            try
            {
                // Spawn CBURN agent at this position using RandomHumanoidSystem
                // The "CBURNAgent" settings prototype includes the GhostRole component
                var agentEntity = _randomHumanoid.SpawnRandomHumanoid("CBURNAgent", localCoords, "CBURN Agent");
                
                usedPositions.Add(paperPos);
                addedCount++;

                Log.Debug($"Spawned CBURN agent at position {paperPos} in shuttle {ToPrettyString(shuttleUid)}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to spawn CBURN agent at position {paperPos} in shuttle {ToPrettyString(shuttleUid)}: {ex.Message}");
            }
        }

        Log.Info($"Spawned {addedCount} CBURN agent(s) on shuttle {ToPrettyString(shuttleUid)}");
        return addedCount;
    }
}
