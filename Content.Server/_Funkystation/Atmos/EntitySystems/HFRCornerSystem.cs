// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Funkystation.Atmos.Components;
using Robust.Shared.Map.Components;
using Content.Shared._Funkystation.Atmos.Visuals;
using Content.Server._Funkystation.Atmos.HFR.Systems;
using System.Numerics;
using Robust.Shared.Maths;

namespace Content.Server._Funkystation.Atmos.Systems;

public sealed class HFRCornerSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HFRCoreSystem _coreSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly HypertorusFusionReactorSystem _hfrSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HFRCornerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<HFRCornerComponent, AnchorStateChangedEvent>(OnAnchorChanged);
    }

    private void OnStartup(EntityUid uid, HFRCornerComponent corner, ComponentStartup args)
    {
        if (TryAlignToCore(uid, corner))
        {
            TryFindCore(uid, corner);
        }
        else
        {
            TryAlignToSidePart(uid, corner);
        }
    }

    private void OnAnchorChanged(EntityUid uid, HFRCornerComponent corner, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            if (corner.CoreUid != null)
            {
                if (EntityManager.TryGetComponent<HFRCoreComponent>(corner.CoreUid, out var coreComp))
                {
                    corner.IsActive = false;
                    if (TryComp<AppearanceComponent>(uid, out var appearance))
                    {
                        _appearanceSystem.SetData(uid, HFRVisuals.IsActive, false, appearance);
                    }

                    coreComp.CornerUids.Remove(uid);
                    _hfrSystem.ToggleActiveState(corner.CoreUid.Value, coreComp, false);
                }
                corner.CoreUid = null;
            }
        }
        else
        {
            TryFindCore(uid, corner);
        }
    }

    private bool TryAlignToCore(EntityUid uid, HFRCornerComponent corner)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return false;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        var cornerCoords = _transformSystem.GetMapCoordinates(uid);
        var cornerTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, cornerCoords);

        foreach (var offset in HFRCoreSystem.DiagonalOffsets)
        {
            var targetTile = cornerTile + offset;
            var entities = new HashSet<Entity<HFRCoreComponent>>();
            _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

            foreach (var (entity, coreComp) in entities)
            {
                if (TryComp<TransformComponent>(entity, out var entityXform) && entityXform.Anchored && entityXform.GridUid == gridUid)
                {
                    float rotation = offset switch
                    {
                        { X: -1, Y: 1 } => 0f,              // Top-left: North
                        { X: 1, Y: 1 } => 3 * MathF.PI / 2,     // Top-right: East
                        { X: 1, Y: -1 } => MathF.PI,        // Bottom-right: South
                        { X: -1, Y: -1 } => MathF.PI / 2, // Bottom-left: West
                        _ => 0f
                    };

                    _transformSystem.SetLocalRotation(uid, new Angle(rotation), xform);
                    return true;
                }
            }
        }

        return false;
    }

    private void TryAlignToSidePart(EntityUid uid, HFRCornerComponent corner)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var cornerCoords = _transformSystem.GetMapCoordinates(uid);
        var cornerTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, cornerCoords);

        foreach (var offset in HFRCoreSystem.CardinalOffsets)
        {
            var targetTile = cornerTile + offset;
            var entities = new HashSet<Entity<HFRSidePartComponent>>();
            _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

            foreach (var (entity, _) in entities)
            {
                if (TryComp<TransformComponent>(entity, out var sideXform) && sideXform.Anchored && sideXform.GridUid == gridUid)
                {
                    var sideRotation = sideXform.LocalRotation;
                    var sideDir = sideRotation.GetCardinalDir();
                    float? cornerRotation = null;

                    if (offset == new Vector2i(1, 0)) // Right
                    {
                        if (sideDir == Direction.South) // South
                            cornerRotation = 3 * MathF.PI / 2; // West
                        else if (sideDir == Direction.North) // North
                            cornerRotation = MathF.PI; // North
                    }
                    else if (offset == new Vector2i(-1, 0)) // Left
                    {
                        if (sideDir == Direction.South) // South
                            cornerRotation = 0f; // South
                        else if (sideDir == Direction.North) // North
                            cornerRotation = MathF.PI / 2; // East
                    }
                    else if (offset == new Vector2i(0, -1)) // Below
                    {
                        if (sideDir == Direction.West) // West
                            cornerRotation = MathF.PI; // North
                        else if (sideDir == Direction.East) // East
                            cornerRotation = MathF.PI / 2; // East
                    }
                    else if (offset == new Vector2i(0, 1)) // Above
                    {
                        if (sideDir == Direction.West) // West
                            cornerRotation = 3 * MathF.PI / 2; // West
                        else if (sideDir == Direction.East) // East
                            cornerRotation = 0f; // South
                    }

                    if (cornerRotation.HasValue)
                    {
                        _transformSystem.SetLocalRotation(uid, new Angle(cornerRotation.Value), xform);
                    }

                    return;
                }
            }
        }
    }

    private void TryFindCore(EntityUid uid, HFRCornerComponent corner)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var cornerCoords = _transformSystem.GetMapCoordinates(uid);
        var cornerTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, cornerCoords);

        foreach (var offset in HFRCoreSystem.DiagonalOffsets)
        {
            var targetTile = cornerTile + offset;
            var entities = new HashSet<Entity<HFRCoreComponent>>();
            _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

            foreach (var (entity, coreComp) in entities)
            {
                if (TryComp<TransformComponent>(entity, out var entityXform) && entityXform.Anchored && entityXform.GridUid == gridUid)
                {
                    _coreSystem.TryLinkCorner(entity, coreComp, uid, corner);
                    break;
                }
            }
        }
    }
}