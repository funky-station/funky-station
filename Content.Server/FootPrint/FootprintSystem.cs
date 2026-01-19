// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Content.Shared.Footprint;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids;
using Content.Shared.Fluids.Components;
using Content.Shared.Standing;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Footprint;

public sealed class FootprintSystem : EntitySystem
{
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedPuddleSystem _puddle = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!; // Added dependency

    public static readonly FixedPoint2 MaxFootprintVolumeOnTile = 50;
    public static readonly EntProtoId FootprintPrototypeId = "Footprint";
    public const string FootprintOwnerSolution = "print";
    public const string FootprintSolution = "print";
    public const string PuddleSolution = "puddle";

    private static readonly string[] DraggingStates =
    [
        "dragging-1",
        "dragging-2",
        "dragging-3",
        "dragging-4",
        "dragging-5"
    ];

    public override void Initialize()
    {
        SubscribeLocalEvent<FootprintComponent, FootprintCleanEvent>(OnFootprintClean);
        SubscribeLocalEvent<FootprintOwnerComponent, MoveEvent>(OnMove);
        SubscribeLocalEvent<PuddleComponent, MapInitEvent>(OnMapInit);
    }

    private void OnFootprintClean(Entity<FootprintComponent> entity, ref FootprintCleanEvent e)
    {
        // When cleaned, turn back into a puddle (or simply vanish if volume is low, handled by puddle logic)
        ToPuddle(entity);
    }

    private void OnMove(Entity<FootprintOwnerComponent> entity, ref MoveEvent e)
    {
        if (!e.OldPosition.IsValid(EntityManager) || !e.NewPosition.IsValid(EntityManager))
            return;

        var oldPosition = _transform.ToMapCoordinates(e.OldPosition).Position;
        var newPosition = _transform.ToMapCoordinates(e.NewPosition).Position;

        entity.Comp.Distance += Vector2.Distance(newPosition, oldPosition);

        var standing = TryComp<StandingStateComponent>(entity, out var standingState) && standingState.CurrentState == StandingState.Standing;
        var requiredDistance = standing ? entity.Comp.FootDistance : entity.Comp.BodyDistance;

        if (entity.Comp.Distance < requiredDistance)
            return;

        entity.Comp.Distance -= requiredDistance;

        var transform = Transform(entity);
        if (transform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var gridComponent))
            return;

        // Calculate offset for left/right steps
        EntityCoordinates coordinates = new(entity, standing ? entity.Comp.NextFootOffset : 0, 0);
        entity.Comp.NextFootOffset = -entity.Comp.NextFootOffset;

        var tile = _map.CoordinatesToTile(gridUid, gridComponent, coordinates);

        // Interact with existing puddle to pick up liquid?
        if (TryPuddleInteraction(entity, (gridUid, gridComponent), tile, standing))
            return;

        Angle rotation;
        if (!standing)
        {
            var oldLocalPosition = _map.WorldToLocal(gridUid, gridComponent, oldPosition);
            var newLocalPosition = _map.WorldToLocal(gridUid, gridComponent, newPosition);
            rotation = (newLocalPosition - oldLocalPosition).ToAngle();
        }
        else
        {
            rotation = transform.LocalRotation;
        }

        FootprintInteraction(entity, (gridUid, gridComponent), tile, coordinates, rotation, standing);
    }

    private bool TryPuddleInteraction(Entity<FootprintOwnerComponent> entity, Entity<MapGridComponent> grid, Vector2i tile, bool standing)
    {
        if (!TryGetAnchoredEntity<PuddleComponent>(grid, tile, out var puddle))
            return false;

        if (!_solution.TryGetSolution(puddle.Value.Owner, PuddleSolution, out var puddleSolution, out _))
            return false;

        // Ensure mob has a solution container to store the liquid on their feet/body
        var maxVol = standing ? entity.Comp.MaxFootVolume : entity.Comp.MaxBodyVolume;
        if (!_solution.EnsureSolutionEntity(entity.Owner, FootprintOwnerSolution, out _, out var solution, FixedPoint2.Max(entity.Comp.MaxFootVolume, entity.Comp.MaxBodyVolume)))
            return false;

        // 1. Wash feet: Transfer FROM Feet TO Puddle.
        _solution.TryTransferSolution(puddleSolution.Value, solution.Value.Comp.Solution, GetFootprintVolume(entity, solution.Value));

        // 2. Soak feet: Transfer FROM Puddle TO Feet.
        var availableSpace = FixedPoint2.Max(0, maxVol - solution.Value.Comp.Solution.Volume);
        _solution.TryTransferSolution(solution.Value, puddleSolution.Value.Comp.Solution, availableSpace);

        _solution.UpdateChemicals(puddleSolution.Value, false);
        return true;
    }

    private void FootprintInteraction(Entity<FootprintOwnerComponent> entity, Entity<MapGridComponent> grid, Vector2i tile, EntityCoordinates coordinates, Angle rotation, bool standing)
    {
        if (!_solution.TryGetSolution(entity.Owner, FootprintOwnerSolution, out var solution, out _))
            return;

        var volume = standing ? GetFootprintVolume(entity, solution.Value) : GetBodyprintVolume(entity, solution.Value);

        if (volume < entity.Comp.MinFootprintVolume)
            return;

        // Check if there is already a footprint entity on this tile
        if (!TryGetAnchoredEntity<FootprintComponent>(grid, tile, out var footprint))
        {
            var footprintEntity = SpawnAtPosition(FootprintPrototypeId, coordinates);
            footprint = (footprintEntity, Comp<FootprintComponent>(footprintEntity));
        }

        if (!_solution.EnsureSolutionEntity(footprint.Value.Owner, FootprintSolution, out _, out var footprintSolution, MaxFootprintVolumeOnTile))
            return;

        // Determine color based on solution
        var color = solution.Value.Comp.Solution.GetColor(_prototype).WithAlpha((float)volume / (float)(standing ? entity.Comp.MaxFootprintVolume : entity.Comp.MaxBodyprintVolume) / 2f);

        // Transfer solution from mob to floor
        _solution.TryTransferSolution(footprintSolution.Value, solution.Value.Comp.Solution, volume);

        // If too much liquid, turn into a real puddle
        if (footprintSolution.Value.Comp.Solution.Volume >= MaxFootprintVolumeOnTile)
        {
            var footprintSolutionClone = footprintSolution.Value.Comp.Solution.Clone();
            Del(footprint);
            _puddle.TrySpillAt(coordinates, footprintSolutionClone, out _, false);
            return;
        }

        // Add visual layer data
        var gridCoords = _map.LocalToGrid(grid, grid, coordinates);
        var x = gridCoords.X / grid.Comp.TileSize;
        var y = gridCoords.Y / grid.Comp.TileSize;
        var halfTileSize = grid.Comp.TileSize / 2f;

        // Normalize offset relative to tile center
        x -= MathF.Floor(x) + halfTileSize;
        y -= MathF.Floor(y) + halfTileSize;

        // Pick state
        var state = standing ? "foot" : _random.Pick(DraggingStates);

        footprint.Value.Comp.Footprints.Add(new(new(x, y), rotation, color, state));
        Dirty(footprint.Value);

        if (TryGetNetEntity(footprint, out var netFootprint))
            RaiseNetworkEvent(new FootprintChangedEvent(netFootprint.Value), Filter.Pvs(footprint.Value));
    }

    private void OnMapInit(Entity<PuddleComponent> entity, ref MapInitEvent e)
    {
        // If a puddle spawns on top of a footprint, consume the footprint
        if (HasComp<FootprintComponent>(entity)) return;

        var transform = Transform(entity);
        if (transform.GridUid is not {} gridUid || !TryComp<MapGridComponent>(gridUid, out var gridComponent)) return;

        var tile = _map.CoordinatesToTile(gridUid, gridComponent, transform.Coordinates);
        if (!TryGetAnchoredEntity<FootprintComponent>((gridUid, gridComponent), tile, out var footprint)) return;

        ToPuddle(footprint.Value, transform.Coordinates);
    }

    private void ToPuddle(EntityUid footprint, EntityCoordinates? coordinates = null)
    {
        coordinates ??= Transform(footprint).Coordinates;

        if (_solution.TryGetSolution(footprint, FootprintSolution, out _, out var footprintSolution))
        {
            var clone = footprintSolution.Clone();
            Del(footprint);
            _puddle.TrySpillAt(coordinates.Value, clone, out _, false);
        }
        else
        {
            Del(footprint);
        }
    }

    private static FixedPoint2 GetFootprintVolume(Entity<FootprintOwnerComponent> entity, Entity<SolutionComponent> solution)
    {
        return FixedPoint2.Min(solution.Comp.Solution.Volume, (entity.Comp.MaxFootprintVolume - entity.Comp.MinFootprintVolume) * (solution.Comp.Solution.Volume / entity.Comp.MaxFootVolume) + entity.Comp.MinFootprintVolume);
    }

    private static FixedPoint2 GetBodyprintVolume(Entity<FootprintOwnerComponent> entity, Entity<SolutionComponent> solution)
    {
        return FixedPoint2.Min(solution.Comp.Solution.Volume, (entity.Comp.MaxBodyprintVolume - entity.Comp.MinBodyprintVolume) * (solution.Comp.Solution.Volume / entity.Comp.MaxBodyVolume) + entity.Comp.MinBodyprintVolume);
    }

    private bool TryGetAnchoredEntity<T>(Entity<MapGridComponent> grid, Vector2i pos, [NotNullWhen(true)] out Entity<T>? entity) where T : IComponent
    {
        var anchoredEnumerator = _map.GetAnchoredEntitiesEnumerator(grid, grid, pos);
        var entityQuery = GetEntityQuery<T>();

        while (anchoredEnumerator.MoveNext(out var ent))
        {
            if (entityQuery.TryComp(ent, out var comp))
            {
                entity = (ent.Value, comp);
                return true;
            }
        }
        entity = null;
        return false;
    }
}
