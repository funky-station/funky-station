// SPDX-FileCopyrightText: 2022 Alex Evgrashin <aevgrashin@yandex.ru>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 eoineoineoin <github@eoinrul.es>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.Radiation.Components;
using Content.Server.Radiation.Events;
using Content.Shared.Radiation.Components;
using Content.Shared.Radiation.Systems;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Radiation.Systems;

// main algorithm that fire radiation rays to target
public partial class RadiationSystem
{
    private List<Entity<MapGridComponent>> _grids = new();
    private Dictionary<EntityUid, Dictionary<Vector2i, float>> _tileRadiationCache = new();

    private readonly record struct SourceData(
        float Intensity,
        Entity<RadiationSourceComponent, TransformComponent> Entity,
        Vector2 WorldPosition,
        List<Entity<MapGridComponent>> Grids)
    {
        public EntityUid? GridUid => Entity.Comp2.GridUid;
        public float Slope => Entity.Comp1.Slope;
        public TransformComponent Transform => Entity.Comp2;
    }

    private void UpdateGridcast()
    {
        // should we save debug information into rays?
        // if there is no debug sessions connected - just ignore it
        var debug = _debugSessions.Count > 0;

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        _sources.Clear();
        _tileRadiationCache.Clear();
        _sources.EnsureCapacity(EntityManager.Count<RadiationSourceComponent>());

        var sources = EntityQueryEnumerator<RadiationSourceComponent, TransformComponent>();
        var destinations = EntityQueryEnumerator<RadiationReceiverComponent, TransformComponent>();

        while (sources.MoveNext(out var uid, out var source, out var xform))
        {
            if (!source.Enabled)
                continue;

            var worldPos = _transform.GetWorldPosition(xform);

            // Intensity is scaled by stack size.
            var intensity = source.Intensity * _stack.GetCount(uid);

            // Apply rad modifier if the source is enclosed within a radiation blocking container
            // Note that this also applies to receivers, and it doesn't bother to check if the container sits between them.
            // I.e., a source & receiver in the same blocking container will get double-blocked, when no blocking should be applied.
            intensity = GetAdjustedRadiationIntensity(uid, intensity);

            // Precompute grids within GridcastMaxDistance for this source
            var grids = new List<Entity<MapGridComponent>>();
            var box = new Box2(worldPos - new Vector2(GridcastMaxDistance, GridcastMaxDistance),
                               worldPos + new Vector2(GridcastMaxDistance, GridcastMaxDistance));
            _mapManager.FindGridsIntersecting(xform.MapID, box, ref grids, true);

            _sources.Add(new(intensity, (uid, source, xform), worldPos, grids));
        }

        var debugRays = debug ? new List<DebugRadiationRay>() : null;
        var receiversTotalRads = new ValueList<(Entity<RadiationReceiverComponent>, float)>();

        // TODO RADIATION Parallelize
        // Would need to give receiversTotalRads a fixed size.
        // Also the _grids list needs to be local to a job. (or better yet cached in SourceData)
        // And I guess disable parallelization when debugging to make populating the debug List<RadiationRay> easier.
        // Or just make it threadsafe?
        while (destinations.MoveNext(out var destUid, out var dest, out var destTrs))
        {
            var destWorld = _transform.GetWorldPosition(destTrs);

            var rads = 0f;
            foreach (var source in _sources)
            {
                // send ray towards destination entity
                if (Irradiate(source, destUid, destTrs, destWorld, debug) is not {} ray)
                    continue;

                // add rads to total rad exposure
                if (ray.ReachedDestination)
                    rads += ray.Rads;

                if (!debug)
                    continue;

                debugRays!.Add(new DebugRadiationRay(
                    ray.MapId,
                    GetNetEntity(ray.SourceUid),
                    ray.Source,
                    GetNetEntity(ray.DestinationUid),
                    ray.Destination,
                    ray.Rads,
                    ray.Blockers ?? new())
                );
            }
            if (destTrs.GridUid is { } gridUid && _gridQuery.TryGetComponent(gridUid, out var grid))
            {
                var tilePos = grid.TileIndicesFor(destTrs.Coordinates);
                if (!_tileRadiationCache.ContainsKey(gridUid))
                    _tileRadiationCache[gridUid] = new();
                _tileRadiationCache[gridUid][tilePos] = rads;
            }
            rads = GetAdjustedRadiationIntensity(destUid, rads);

            receiversTotalRads.Add(((destUid, dest), rads));
        }

        // update information for debug overlay
        var elapsedTime = stopwatch.Elapsed.TotalMilliseconds;
        var totalSources = _sources.Count;
        var totalReceivers = receiversTotalRads.Count;
        UpdateGridcastDebugOverlay(elapsedTime, totalSources, totalReceivers, debugRays);

        // send rads to each entity
        foreach (var (receiver, rads) in receiversTotalRads)
        {
            // update radiation value of receiver
            // if no radiation rays reached target, that will set it to 0
            receiver.Comp.CurrentRadiation = rads;

            // also send an event with combination of total rad
            if (rads > 0)
                IrradiateEntity(receiver, rads, GridcastUpdateRate);
        }

        // raise broadcast event that radiation system has updated
        RaiseLocalEvent(new RadiationSystemUpdatedEvent());
    }

    private RadiationRay? Irradiate(SourceData source,
        EntityUid destUid,
        TransformComponent destTrs,
        Vector2 destWorld,
        bool saveVisitedTiles)
    {
        // lets first check that source and destination on the same map
        if (source.Transform.MapID != destTrs.MapID)
            return null;

        var mapId = destTrs.MapID;

        // get direction from rad source to destination and its distance
        var dir = destWorld - source.WorldPosition;
        var dist = dir.Length();

        // check if receiver is too far away
        if (dist > GridcastMaxDistance)
            return null;

        // will it even reach destination considering distance penalty
        var rads = source.Intensity - source.Slope * dist;
        if (rads < MinIntensity)
            return null;

        // create a new radiation ray from source to destination
        // at first we assume that it doesn't hit any radiation blockers
        // and has only distance penalty
        var ray = new RadiationRay(mapId, source.Entity, source.WorldPosition, destUid, destWorld, rads);

        // if source and destination on the same grid it's possible that
        // between them can be another grid (ie. shuttle in center of donut station)
        // however we can do simplification and ignore that case
        if (GridcastSimplifiedSameGrid && destTrs.GridUid is {} gridUid && source.GridUid == gridUid)
        {
            if (!_gridQuery.TryGetComponent(gridUid, out var gridComponent))
                return ray;
            return Gridcast((gridUid, gridComponent, Transform(gridUid)), ref ray, saveVisitedTiles, source.Transform, destTrs);
        }

        foreach (var grid in source.Grids)
        {
            ray = Gridcast((grid.Owner, grid.Comp, Transform(grid)), ref ray, saveVisitedTiles, source.Transform, destTrs);

            // looks like last grid blocked all radiation
            // we can return right now
            if (ray.Rads <= 0)
                return ray;
        }

        return ray;
    }

    private RadiationRay Gridcast(
        Entity<MapGridComponent, TransformComponent> grid,
        ref RadiationRay ray,
        bool saveVisitedTiles,
        TransformComponent sourceTrs,
        TransformComponent destTrs)
    {
        var blockers = saveVisitedTiles ? new List<(Vector2i, float)>() : null;
        if (!_resistanceQuery.TryGetComponent(grid.Owner, out var resistance))
            return ray;
        var resistanceMap = resistance.ResistancePerTile;

        // get coordinate of source and destination in grid coordinates

        // TODO Grid overlap. This currently assumes the grid is always parented directly to the map (local matrix == world matrix).
        // If ever grids are allowed to overlap, this might no longer be true. In that case, this should precompute and cache
        // inverse world matrices.

        Vector2 srcLocal = sourceTrs.ParentUid == grid.Owner
            ? sourceTrs.LocalPosition
            : Vector2.Transform(ray.Source, grid.Comp2.InvLocalMatrix);

        Vector2 dstLocal = destTrs.ParentUid == grid.Owner
            ? destTrs.LocalPosition
            : Vector2.Transform(ray.Destination, grid.Comp2.InvLocalMatrix);

        Vector2i sourceGrid = new(
            (int) Math.Floor(srcLocal.X / grid.Comp1.TileSize),
            (int) Math.Floor(srcLocal.Y / grid.Comp1.TileSize));

        Vector2i destGrid = new(
            (int) Math.Floor(dstLocal.X / grid.Comp1.TileSize),
            (int) Math.Floor(dstLocal.Y / grid.Comp1.TileSize));

        // iterate tiles in grid line from source to destination
        var line = new GridLineEnumerator(sourceGrid, destGrid);
        while (line.MoveNext())
        {
            var point = line.Current;
            if (!resistanceMap.TryGetValue(point, out var resData))
                continue;
            ray.Rads -= resData;

            // save data for debug
            if (saveVisitedTiles)
                blockers!.Add((point, ray.Rads));

            // no intensity left after blocker
            if (ray.Rads <= MinIntensity)
            {
                ray.Rads = 0;
                break;
            }
        }

        if (!saveVisitedTiles || blockers!.Count <= 0)
            return ray;

        // save data for debug if needed
        ray.Blockers ??= new();
        ray.Blockers.Add(GetNetEntity(grid.Owner), blockers);

        return ray;
    }

    private float GetAdjustedRadiationIntensity(EntityUid uid, float rads)
    {
        var child = uid;
        var xform = Transform(uid);
        var parent = xform.ParentUid;

        while (parent.IsValid())
        {
            var parentXform = Transform(parent);
            var childMeta = MetaData(child);

            if ((childMeta.Flags & MetaDataFlags.InContainer) != MetaDataFlags.InContainer)
            {
                child = parent;
                parent = parentXform.ParentUid;
                continue;
            }

            if (_blockerQuery.TryComp(xform.ParentUid, out var blocker))
            {
                rads -= blocker.RadResistance;
                if (rads < 0)
                    return 0;
            }

            child = parent;
            parent = parentXform.ParentUid;
        }

        return rads;
    }
}
