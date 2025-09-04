// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.Actions.Events; // For MalfAiRoboticsFactoryActionEvent
using Content.Shared.DoAfter;
using Content.Shared._Funkystation.Factory;
using Content.Shared.MalfAI;
using Robust.Shared.GameObjects;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using System;
using System.Collections.Generic;

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
    private static readonly ISawmill Sawmill = Logger.GetSawmill("ai.build.system");

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<AIBuildRequestEvent>(OnBuildRequest);
        SubscribeLocalEvent<Content.Shared.MalfAI.MalfAiMarkerComponent, AIBuildDoAfterEvent>(OnBuildDoAfter);

        Sawmill.Info("[DEBUG_LOG] AIBuildSystem initialized.");
        Sawmill.Info("[DEBUG_LOG] Subscribed to AIBuildRequestEvent and AIBuildDoAfterEvent with MalfAiMarkerComponent");
    }

    /// <summary>
    /// Handles build requests from AI entities
    /// </summary>
    private void OnBuildRequest(AIBuildRequestEvent args)
    {
        var requester = args.Requester;
        var target = args.Target;
        var prototype = args.Prototype;

        Sawmill.Info($"[DEBUG_LOG] AIBuildSystem.OnBuildRequest called");
        Sawmill.Info($"[DEBUG_LOG] Build request details - Requester: {ToPrettyString(requester)}, Target: {target}, Prototype: '{prototype}'");

        // Validate prototype exists
        var prototypeExists = _prototypes.HasIndex<EntityPrototype>(prototype);
        Sawmill.Info($"[DEBUG_LOG] Prototype '{prototype}' exists: {prototypeExists}");
        if (!prototypeExists)
        {
            Sawmill.Error($"[DEBUG_LOG] AIBuild: Invalid prototype '{prototype}' requested by {ToPrettyString(requester)}");
            return;
        }

        // Validate coordinates
        var coordinatesValid = target.IsValid(EntityManager);
        Sawmill.Info($"[DEBUG_LOG] Target coordinates valid: {coordinatesValid}");
        if (!coordinatesValid)
        {
            Sawmill.Warning($"[DEBUG_LOG] AIBuild: Invalid coordinates {target} for prototype '{prototype}'");
            return;
        }

        // Validate tile is free
        var tileFree = IsTileFree(target);
        Sawmill.Info($"[DEBUG_LOG] Tile at {target} is free: {tileFree}");
        if (!tileFree)
        {
            Sawmill.Warning($"[DEBUG_LOG] AIBuild: Tile at {target} is occupied, cannot build '{prototype}'");
            return;
        }

        // Start building process with DoAfter
        var doAfterEvent = new AIBuildDoAfterEvent(GetNetCoordinates(target), prototype);
        var delay = TimeSpan.FromSeconds(3.0f); // 3 second build time
        Sawmill.Info($"[DEBUG_LOG] Creating DoAfter with {delay.TotalSeconds}s delay");

        // Try to get the AI's visible eye entity (RemoteEntity) for DoAfter display
        EntityUid doAfterUser = requester;
        var aiCore = SharedMalfAiHelpers.ResolveAiCoreFrom(EntityManager, _transform, requester);
        if (aiCore != EntityUid.Invalid &&
            TryComp<Content.Shared.Silicons.StationAi.StationAiCoreComponent>(aiCore, out var coreComp) &&
            coreComp.RemoteEntity.HasValue)
        {
            doAfterUser = coreComp.RemoteEntity.Value;
            Sawmill.Info($"[DEBUG_LOG] Resolved AI core: {ToPrettyString(aiCore)}, using RemoteEntity for DoAfter display: {ToPrettyString(doAfterUser)}");
        }
        else
        {
            Sawmill.Info($"[DEBUG_LOG] Could not resolve AI core or RemoteEntity (core: {ToPrettyString(aiCore)}), using requester for DoAfter: {ToPrettyString(doAfterUser)}");
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

        var doAfterStarted = _doAfter.TryStartDoAfter(doAfterArgs);
        Sawmill.Info($"[DEBUG_LOG] DoAfter start attempt result: {doAfterStarted}");

        if (doAfterStarted)
        {
            Sawmill.Info($"[DEBUG_LOG] AIBuild: Started building '{prototype}' for {ToPrettyString(requester)} at {target}");
        }
        else
        {
            Sawmill.Warning($"[DEBUG_LOG] AIBuild: Failed to start DoAfter for '{prototype}' build request");
        }
    }

    /// <summary>
    /// Handles completion of the build process
    /// </summary>
    private void OnBuildDoAfter(EntityUid uid, Content.Shared.MalfAI.MalfAiMarkerComponent component, AIBuildDoAfterEvent args)
    {
        Sawmill.Info($"[DEBUG_LOG] AIBuildSystem.OnBuildDoAfter called for prototype '{args.Prototype}'");
        Sawmill.Info($"[DEBUG_LOG] DoAfter cancelled: {args.Cancelled}");


        if (args.Cancelled)
        {
            Sawmill.Debug($"[DEBUG_LOG] AIBuild: Build cancelled for prototype '{args.Prototype}'");
            return;
        }

        var location = GetCoordinates(args.Location);
        Sawmill.Info($"[DEBUG_LOG] Build completion location: {location}");

        // Final validation before spawning
        var prototypeExists = _prototypes.HasIndex<EntityPrototype>(args.Prototype);
        Sawmill.Info($"[DEBUG_LOG] Final prototype validation - '{args.Prototype}' exists: {prototypeExists}");
        if (!prototypeExists)
        {
            Sawmill.Error($"[DEBUG_LOG] AIBuild: Invalid prototype '{args.Prototype}' at completion");
            return;
        }

        var tileFree = IsTileFree(location);
        Sawmill.Info($"[DEBUG_LOG] Final tile validation - location free: {tileFree}");
        if (!tileFree)
        {
            Sawmill.Warning($"[DEBUG_LOG] AIBuild: Tile at {location} became occupied during build");
            return;
        }

        try
        {
            Sawmill.Info($"[DEBUG_LOG] Attempting to spawn entity '{args.Prototype}' at {location}");
            // Spawn the entity
            var spawned = EntityManager.SpawnEntity(args.Prototype, location);
            Sawmill.Info($"[DEBUG_LOG] Entity spawned successfully: {ToPrettyString(spawned)}");

            // If this is a robotics factory grid, remember who built it so we can assign borgs later.
            var isFactory = false;
            if (HasComp<Content.Shared._Funkystation.Factory.Components.RoboticsFactoryGridComponent>(spawned))
            {
                isFactory = true;
                var owner = EnsureComp<Content.Server._Funkystation.Factory.Components.MalfFactoryOwnerComponent>(spawned);
                owner.Controller = uid; // uid is the AI entity that received the DoAfter completion
                Dirty(spawned, owner);
                Sawmill.Info($"[DEBUG_LOG] Tagged factory {ToPrettyString(spawned)} with owner {ToPrettyString(uid)}");
            }

            // Anchor the entity if possible
            TryAnchorEntity(spawned);
            Sawmill.Info($"[DEBUG_LOG] Anchoring attempt completed");

            // On success, remove the Robotics Factory action from the Malf AI that built it.
            if (isFactory)
                RemoveRoboticsFactoryAction(uid);

            Sawmill.Info($"[DEBUG_LOG] AIBuild: Successfully spawned '{args.Prototype}' as {ToPrettyString(spawned)} at {location}");
        }
        catch (Exception ex)
        {
            Sawmill.Error($"[DEBUG_LOG] AIBuild: Failed to spawn '{args.Prototype}' at {location}: {ex}");
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

        // Check for anchored entities (existing structures, doors, etc.)
        foreach (var _ in grid.GetAnchoredEntities(tile))
            return false;

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

        Sawmill.Debug($"AIBuild: Anchored entity {ToPrettyString(entity)}");
    }
}
