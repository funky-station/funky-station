// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server._Funkystation.Factory.Components;
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.Factory;
using Content.Shared._Funkystation.Factory.Components;
using Content.Shared.MalfAI;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Tag;

namespace Content.Server._Funkystation.Factory.Systems;

/// <summary>
/// Event to request building a prototype at a specific location
/// </summary>
public sealed partial class AIBuildRequestEvent : EntityEventArgs
{
    public EntityUid Requester { get; }
    public EntityCoordinates Target { get; }
    public string Prototype { get; }

    public AIBuildRequestEvent(EntityUid requester, EntityCoordinates target, string prototype)
    {
        Requester = requester;
        Target = target;
        Prototype = prototype;
    }
}


/// <summary>
/// System that handles AI building requests by spawning prototypes at specified locations
/// </summary>
public sealed partial class AIBuildSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly Content.Shared.Actions.SharedActionsSystem _actions = default!;
    [Dependency] private readonly TagSystem _tagSystem = default!;
    private static readonly ISawmill Sawmill = Logger.GetSawmill("ai.build.system");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AIBuildRequestEvent>(OnBuildRequest);
        SubscribeLocalEvent<MalfAiMarkerComponent, AIBuildDoAfterEvent>(OnBuildDoAfter);
    }

    /// <summary>
    /// Handles build requests from AI entities
    /// </summary>
    private void OnBuildRequest(AIBuildRequestEvent args)
    {
        var requester = args.Requester;
        var target = args.Target;
        var prototype = args.Prototype;


        // Validate coordinates
        if (!target.IsValid(EntityManager))
        {
            Sawmill.Warning($"AIBuild: Invalid coordinates {target} for prototype '{prototype}'");
            return;
        }

        // Validate tile is free
        if (!IsTileFree(target))
        {
            Sawmill.Warning($"AIBuild: Tile at {target} is occupied, cannot build '{prototype}'");
            return;
        }

        // Start building process with DoAfter
        var doAfterEvent = new AIBuildDoAfterEvent(GetNetCoordinates(target), prototype);
        var delay = TimeSpan.FromSeconds(3.0f); // 3 second build time

        // Try to get the AI's visible eye entity (RemoteEntity) for DoAfter display
        EntityUid doAfterUser = requester;
        var aiCore = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _transform, requester);
        if (aiCore != EntityUid.Invalid &&
            TryComp<Content.Shared.Silicons.StationAi.StationAiCoreComponent>(aiCore, out var coreComp) &&
            coreComp.RemoteEntity.HasValue)
        {
            doAfterUser = coreComp.RemoteEntity.Value;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, doAfterUser, delay, doAfterEvent, eventTarget: requester)
        {
            BreakOnMove = true, // Cancel if the AI eye moves during the build
            BreakOnDamage = true,
            BreakOnHandChange = false,
            CancelDuplicate = false,
            BlockDuplicate = false,
            NeedHand = false,
            Hidden = false
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
        {
            Sawmill.Warning($"AIBuild: Failed to start DoAfter for '{prototype}' build request");
        }
    }

    /// <summary>
    /// Handles completion of the build process
    /// </summary>
    private void OnBuildDoAfter(EntityUid uid, Content.Shared.MalfAI.MalfAiMarkerComponent component, AIBuildDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var location = GetCoordinates(args.Location);


        if (!IsTileFree(location))
        {
            Sawmill.Warning($"AIBuild: Tile at {location} became occupied during build");
            return;
        }

        try
        {
            // Spawn the entity
            var spawned = EntityManager.SpawnEntity(args.Prototype, location);

            // If this is a robotics factory grid, remember who built it so we can assign borgs later.
            var isFactory = false;
            if (HasComp<RoboticsFactoryGridComponent>(spawned))
            {
                isFactory = true;
                var owner = EnsureComp<MalfFactoryOwnerComponent>(spawned);
                owner.Controller = uid; // uid is the AI entity that received the DoAfter completion
            }

            // Anchor the entity if possible
            TryAnchorEntity(spawned);

            // On success, remove the Robotics Factory action from the Malf AI that built it.
            if (isFactory)
                RemoveRoboticsFactoryAction(uid);
        }
        catch (Exception ex)
        {
            Sawmill.Error($"AIBuild: Failed to spawn '{args.Prototype}' at {location}: {ex}");
        }
    }

    private void RemoveRoboticsFactoryAction(EntityUid performer)
    {
        // Remove the Robotics Factory action (ActionMalfAiRoboticsFactory) from the performer.
        // We search via ActionsComponent -> BaseActionComponent.BaseEvent type.
        if (!TryComp<Content.Shared.Actions.ActionsComponent>(performer, out var actionsComp))
            return;

        var toRemove = new List<EntityUid>();
        foreach (var (actId, actComp) in _actions.GetActions(performer, actionsComp))
        {
            if (actComp.BaseEvent is Content.Shared.Actions.Events.MalfAiRoboticsFactoryActionEvent)
                toRemove.Add(actId);
        }

        foreach (var id in toRemove)
            _actions.RemoveAction(performer, id, actionsComp);
    }

    /// <summary>
    /// Checks if a tile is free for building
    /// </summary>
    private bool IsTileFree(EntityCoordinates coordinates)
    {
        if (!coordinates.IsValid(EntityManager))
            return false;

        if (!TryComp<MapGridComponent>(coordinates.EntityId, out var grid))
            return false;

        var tile = grid.TileIndicesFor(coordinates);
        var tileRef = grid.GetTileRef(tile);

        // Check if the tile exists and is not empty space
        if (tileRef.Tile.IsEmpty)
            return false;

        // Check for anchored entities, but allow building on subfloor and wall-mounted entities
        foreach (var entity in grid.GetAnchoredEntities(tile))
        {
            // Allow building over entities with SubFloorHideComponent (cables, pipes, disposal pipes)
            if (HasComp<Content.Shared.SubFloor.SubFloorHideComponent>(entity))
                continue;

            // Allow building over entities with WallMount tag (cameras, lights, wall-mounted devices)
            // I may have forgot a few here, add this tag if noticed missing
            if (_tagSystem.HasTag(entity, "WallMount"))
                continue;

            // Block building on other anchored entities (walls, doors, machines, etc.)
            return false;
        }

        return true;
    }
    /// <summary>
    /// Attempts to anchor an entity if it can be anchored
    /// </summary>
    private void TryAnchorEntity(EntityUid entity)
    {
        if (!TryComp<TransformComponent>(entity, out var transform))
            return;

        if (transform.Anchored)
            return;

        // Try to anchor the entity
        transform.Anchored = true;
    }
}
