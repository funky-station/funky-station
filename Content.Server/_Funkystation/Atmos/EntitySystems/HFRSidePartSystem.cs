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

public sealed class HFRSidePartSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly HFRCoreSystem _coreSystem = default!;
    [Dependency] private readonly EntityLookupSystem _lookupSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<HFRSidePartComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, HFRSidePartComponent sidePart, ComponentStartup args)
    {
        if (TryAlignToCore(uid, sidePart))
            TryFindCore(uid);
        else
            TryAlignToCorner(uid, sidePart);
    }

    private bool TryAlignToCore(EntityUid uid, HFRSidePartComponent sidePart)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return false;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        var sidePartCoords = _transformSystem.GetMapCoordinates(uid);
        var sidePartTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, sidePartCoords);

        foreach (var offset in HFRCoreSystem.CardinalOffsets)
        {
            var targetTile = sidePartTile + offset;
            var entities = new HashSet<Entity<HFRCoreComponent>>();
            _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

            foreach (var (entity, _) in entities)
            {
                if (TryComp<TransformComponent>(entity, out var entityXform) && entityXform.Anchored && entityXform.GridUid == gridUid)
                {
                    float rotation = offset switch
                    {
                        { X: 1, Y: 0 } => 3 * MathF.PI / 2,  // Core right: face West
                        { X: -1, Y: 0 } => MathF.PI / 2,    // Core left: face East
                        { X: 0, Y: 1 } => 0f,               // Core above: face South
                        { X: 0, Y: -1 } => MathF.PI,        // Core below: face North
                        _ => 0f
                    };

                    _transformSystem.SetLocalRotation(uid, new Angle(rotation), xform);
                    return true;
                }
            }
        }

        return false;
    }

    private void TryAlignToCorner(EntityUid uid, HFRSidePartComponent sidePart)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var sidePartCoords = _transformSystem.GetMapCoordinates(uid);
        var sidePartTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, sidePartCoords);

        foreach (var offset in HFRCoreSystem.CardinalOffsets)
        {
            var targetTile = sidePartTile + offset;
            var entities = new HashSet<Entity<HFRCornerComponent>>();
            _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

            foreach (var (entity, _) in entities)
            {
                if (TryComp<TransformComponent>(entity, out var cornerXform) && cornerXform.Anchored && cornerXform.GridUid == gridUid)
                {
                    var cornerRotation = cornerXform.LocalRotation;
                    var cornerDir = cornerRotation.GetCardinalDir();
                    float? sidePartRotation = null;

                    if (offset == new Vector2i(1, 0)) // Corner right
                    {
                        if (cornerDir == Direction.South) // South
                            sidePartRotation = 0f; // South
                        else if (cornerDir == Direction.East) // East
                            sidePartRotation = MathF.PI; // North
                    }
                    else if (offset == new Vector2i(-1, 0)) // Corner left
                    {
                        if (cornerDir == Direction.North) // North
                            sidePartRotation = MathF.PI; // North
                        else if (cornerDir == Direction.West) // West
                            sidePartRotation = 0f; // South
                    }
                    else if (offset == new Vector2i(0, -1)) // Corner below
                    {
                        if (cornerDir == Direction.South) // South
                            sidePartRotation = MathF.PI / 2; // East
                        else if (cornerDir == Direction.West) // West
                            sidePartRotation = 3 * MathF.PI / 2; // West
                    }
                    else if (offset == new Vector2i(0, 1)) // Corner above
                    {
                        if (cornerDir == Direction.North) // North
                            sidePartRotation = 3 * MathF.PI / 2; // West
                        else if (cornerDir == Direction.East) // East
                            sidePartRotation = MathF.PI / 2; // East
                    }

                    if (sidePartRotation.HasValue)
                    {
                        _transformSystem.SetLocalRotation(uid, new Angle(sidePartRotation.Value), xform);
                    }

                    return;
                }
            }
        }
    }

    public void TryFindCore(EntityUid uid)
    {
        if (!TryComp<TransformComponent>(uid, out var xform) || !xform.Anchored)
            return;

        var gridUid = xform.GridUid;
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var rotation = xform.LocalRotation;
        var direction = rotation.GetCardinalDir();
        var offset = -direction.ToIntVec();
        var sidePartCoords = _transformSystem.GetMapCoordinates(uid);
        var sidePartTile = _mapSystem.CoordinatesToTile(gridUid.Value, grid, sidePartCoords);
        var targetTile = sidePartTile + offset;

        var entities = new HashSet<Entity<HFRCoreComponent>>();
        _lookupSystem.GetLocalEntitiesIntersecting(gridUid.Value, targetTile, entities);

        foreach (var (entity, coreComp) in entities)
        {
            if (TryComp<TransformComponent>(entity, out var entityXform) && entityXform.Anchored && entityXform.GridUid == gridUid)
            {
                // Check which component the side part has and pass it to TryLinkComponent
                if (TryComp<HFRConsoleComponent>(uid, out var consoleComp))
                {
                    _coreSystem.TryLinkComponent(entity, coreComp, uid, consoleComp, (core, compUid) => core.ConsoleUid = compUid);
                }
                else if (TryComp<HFRFuelInputComponent>(uid, out var fuelInputComp))
                {
                    _coreSystem.TryLinkComponent(entity, coreComp, uid, fuelInputComp, (core, compUid) => core.FuelInputUid = compUid);
                }
                else if (TryComp<HFRModeratorInputComponent>(uid, out var moderatorInputComp))
                {
                    _coreSystem.TryLinkComponent(entity, coreComp, uid, moderatorInputComp, (core, compUid) => core.ModeratorInputUid = compUid);
                }
                else if (TryComp<HFRWasteOutputComponent>(uid, out var wasteOutputComp))
                {
                    _coreSystem.TryLinkComponent(entity, coreComp, uid, wasteOutputComp, (core, compUid) => core.WasteOutputUid = compUid);
                }

                break;
            }
        }
    }
}