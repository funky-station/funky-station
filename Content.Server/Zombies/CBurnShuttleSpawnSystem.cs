// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using System.Numerics;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Ghost.Roles.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Zombies;
using Robust.Server.GameObjects;
using Robust.Server.Player;
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
public sealed class CBurnShuttleSpawnSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    private const int PlayersPerShuttle = 10;
    private static readonly ResPath ShuttlePath = new("/Maps/_Funkystation/Shuttles/ERTShuttles/ERTBurnWagonV2.yml");

    public override void Initialize()
    {
        base.Initialize();
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
    public void SpawnCBurnShuttles(CBurnShuttleSpawnComponent spawnComp)
    {
        var shuttleCount = CalculateShuttleCount();
        var paperPositions = ParsePaperPositions();

        if (paperPositions.Count == 0)
        {
            Log.Warning("No paper positions found in CBURN shuttle file, cannot spawn ghost roles");
        }

        Log.Info($"Spawning {shuttleCount} CBURN shuttle(s) with {paperPositions.Count} paper position(s)");

        var usedPositions = new HashSet<Vector2>(); // Track used positions across all shuttles

        for (int i = 0; i < shuttleCount; i++)
        {
            // Create new map for this shuttle
            _mapSystem.CreateMap(out var mapId);
            var opts = DeserializationOptions.Default with { InitializeMaps = true };

            if (!_mapLoader.TryLoadGrid(mapId, ShuttlePath, out var grid, opts))
            {
                Log.Error($"Failed to load CBURN shuttle grid {i + 1}/{shuttleCount}");
                continue;
            }

            var shuttleUid = grid.Value.Owner;
            spawnComp.SpawnedShuttles.Add(shuttleUid);
            Dirty(spawnComp);

            Log.Info($"Spawned CBURN shuttle {i + 1}/{shuttleCount} at {ToPrettyString(shuttleUid)}");

            // Add ghost roles at paper positions
            AddGhostRolesToShuttle(shuttleUid, paperPositions, usedPositions);
        }
    }

    /// <summary>
    /// Adds ghost roles to entities at paper positions in the spawned shuttle.
    /// </summary>
    private void AddGhostRolesToShuttle(EntityUid shuttleUid, List<Vector2> paperPositions, HashSet<Vector2> usedPositions)
    {
        if (!TryComp<TransformComponent>(shuttleUid, out var shuttleXform))
            return;

        var shuttleGrid = shuttleUid;
        var addedCount = 0;

        // Find all paper entities in the shuttle
        var paperQuery = EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
        var paperEntities = new List<(EntityUid Uid, Vector2 Position)>();
        
        while (paperQuery.MoveNext(out var uid, out var meta, out var xform))
        {
            // Only look for entities on this shuttle grid
            if (xform.GridUid != shuttleGrid)
                continue;

            // Check if it's a paper entity
            if (meta.EntityPrototype?.ID == "PaperNanoTaskItem")
            {
                paperEntities.Add((uid, xform.LocalPosition));
            }
        }

        // Match paper positions to entities
        foreach (var paperPos in paperPositions)
        {
            // Skip if this position was already used
            if (usedPositions.Contains(paperPos))
                continue;

            EntityUid? targetEntity = null;
            float closestDistance = float.MaxValue;

            // Find the closest paper entity to this position
            foreach (var (uid, entityPos) in paperEntities)
            {
                // Skip if this entity already has a ghost role
                if (HasComp<GhostRoleComponent>(uid))
                    continue;

                var distance = (entityPos - paperPos).Length();
                if (distance < 0.5f && distance < closestDistance)
                {
                    targetEntity = uid;
                    closestDistance = distance;
                }
            }

            // If no paper entity found at this position, skip it
            // (We only add ghost roles to existing papers, as per user requirement)
            if (targetEntity == null)
            {
                Log.Debug($"No paper entity found at position {paperPos} in shuttle {ToPrettyString(shuttleUid)}, skipping");
                continue;
            }

            // Add ghost role components
            var ghostRole = EnsureComp<GhostRoleComponent>(targetEntity.Value);
            EnsureComp<GhostTakeoverAvailableComponent>(targetEntity.Value);

            // Set ghost role properties
            ghostRole.RoleName = "CBURN Team Member";
            ghostRole.RoleDescription = "A member of the Central Command Burn Response Unit, sent to assist with the zombie outbreak.";
            ghostRole.RoleRules = "ghost-role-component-default-rules";
            ghostRole.JobProto = "CBURN";
            ghostRole.MakeSentient = true;
            ghostRole.ReregisterOnGhost = true;

            usedPositions.Add(paperPos);
            addedCount++;

            Log.Debug($"Added ghost role at position {paperPos} in shuttle {ToPrettyString(shuttleUid)}");
        }

        Log.Info($"Added {addedCount} ghost role(s) to CBURN shuttle {ToPrettyString(shuttleUid)}");
    }
}
