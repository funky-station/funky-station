// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Overlays;
using Content.Shared.Inventory.Events;
using Content.Shared.Physics;
using Content.Shared.XRay;
using Content.Shared.Whitelist;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;
using System.Linq;
using System.Numerics;

namespace Content.Client.XRay;

public sealed class ShowXRaySystem : EquipmentHudSystem<ShowXRayComponent>
{
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;

    private XRayOverlay _overlay = default!;

    // used as in reality your eyes focus point is behind you due to having 2 eyes
    // this is meant to simulate this for walls and wallmounts
    private static readonly Vector2 RayOffset = new(0, 1.3f);

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new();

        // --- ADDED: Listen for server updates to the component's state ---
        SubscribeLocalEvent<ShowXRayComponent, ComponentHandleState>(OnHandleState);
    }

    // --- ADDED: This method handles the toggle logic ---
    private void OnHandleState(EntityUid uid, ShowXRayComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not ShowXRayComponentState state)
            return;

        // 1. Update the component's data from the server
        component.Enabled = state.Enabled;
        component.Shader = state.Shader;
        component.EntityRange = state.EntityRange;
        component.TileRange = state.TileRange;

        // 2. Directly modify the list of active overlays
        if (component.Enabled)
        {
            // If we are enabling, ensure the component is in the list of providers
            _overlay.Providers.Add((uid, component));
        }
        else
        {
            // If we are disabling, remove it
            _overlay.Providers.Remove((uid, component));
        }

        // 3. Force the overlay to redraw with the new list
        _overlay.Refresh();
    }


    protected override void UpdateInternal(RefreshEquipmentHudEvent<ShowXRayComponent> args)
    {
        base.UpdateInternal(args);

        if (!_overlayMan.HasOverlay<XRayOverlay>())
            _overlayMan.AddOverlay(_overlay);

        _overlay.Providers.Clear();

        // --- MODIFIED: Ensure we only add enabled components ---
        foreach (var component in args.Components)
        {
            if (component.Enabled)
                _overlay.Providers.Add((component.Owner, component));
        }

        _overlay.Refresh();
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _overlayMan.RemoveOverlay(_overlay);
    }

    public IEnumerable<EntityUid> GetEntities(Entity<ShowXRayComponent> ent)
    {
        if (!Initialized(ent))
            yield break;

        var entities = _lookup.GetEntitiesInRange(ent.Owner, ent.Comp.EntityRange);

        var xrayTransform = Transform(ent.Owner);
        var (xrayPos, xrayRot) = _transform.GetWorldPositionRotation(xrayTransform);
        var xrayMapId = xrayTransform.MapID;

        var rayPos = GetRayPos(xrayPos, xrayRot);

        foreach (var entity in entities)
        {
            if (!_whitelist.CheckBoth(entity, ent.Comp.Blacklist, ent.Comp.Whitelist))
                continue;

            if (!HasComp<SpriteComponent>(entity))
                continue;

            var entityPos = _transform.GetWorldPosition(entity);

            // must be hidden
            if (!EntitiesBlocking(rayPos, xrayMapId, entityPos, x => CanHitEntity(x, entity, xrayPos, xrayRot)))
                continue;

            yield return entity;
        }
    }

    public IEnumerable<TileRef> GetTiles(Entity<ShowXRayComponent> ent)
    {
        if (!Initialized(ent))
            yield break;

        var xrayTransform = Transform(ent.Owner);
        var (xrayPos, xrayRot) = _transform.GetWorldPositionRotation(xrayTransform);
        var xrayMapId = xrayTransform.MapID;

        var radius = new Circle(xrayPos, ent.Comp.TileRange);
        var circle = new PhysShapeCircle(ent.Comp.TileRange, xrayPos);

        List<Entity<MapGridComponent>> grids = new();
        _mapManager.FindGridsIntersecting(xrayMapId, circle, Robust.Shared.Physics.Transform.Empty, ref grids, includeMap: false);

        var rayPos = GetRayPos(xrayPos, xrayRot);

        foreach (var (gridUid, gridComp) in grids)
        {
            var tiles = _map.GetTilesIntersecting(gridUid, gridComp, radius);

            var (gridPos, gridRot) = _transform.GetWorldPositionRotation(gridUid);

            foreach (var tile in tiles)
            {
                var tileLocalPos = _map.ToCenterCoordinates(tile, gridComp);
                var tilePos = gridPos + gridRot.RotateVec(tileLocalPos.Position);

                if (!EntitiesBlocking(rayPos, xrayMapId, tilePos, x => CanHitTile(x, tilePos, xrayPos, xrayRot)))
                    continue;

                yield return tile;
            }
        }
    }

    private bool EntitiesBlocking(Vector2 xrayPos, MapId xrayMapId, Vector2 targetPos, Func<EntityUid, bool> predicate)
    {
        var delta = targetPos - xrayPos;
        var distance = delta.Length();

        if (distance <= float.Epsilon)
            return false;

        var direction = delta.Normalized();

        var ray = new CollisionRay(xrayPos, direction, (int)CollisionGroup.SingularityLayer);
        return _physics.IntersectRayWithPredicate(xrayMapId, ray, distance, e => !predicate(e)).Any();
    }

    private bool CanHitEntity(EntityUid target, EntityUid goal, Vector2 xrayPos, Angle xrayRot)
    {
        if (!IsOcluding(target))
            return false;

        if (target == goal)
            return false;

        var targetPos = _transform.GetWorldPosition(target);

        if (IsBehind(xrayPos, xrayRot, targetPos))
            return false;

        return true;
    }

    private bool CanHitTile(EntityUid target, Vector2 tilePos, Vector2 xrayPos, Angle xrayRot)
    {
        if (!IsOcluding(target))
            return false;

        var targetPos = _transform.GetWorldPosition(target);

        // prevent rendering below the occluder
        if (targetPos.EqualsApprox(tilePos))
            return false;

        if (IsBehind(xrayPos, xrayRot, targetPos))
            return false;

        return true;
    }

    private bool IsOcluding(EntityUid target)
    {
        return TryComp<OccluderComponent>(target, out var occluder) && occluder.Enabled;
    }

    private static bool IsBehind(Vector2 xrayPos, Angle xrayRot, Vector2 targetPos)
    {
        var direction = (targetPos - xrayPos).Normalized();
        var dotProduct = Vector2.Dot(xrayRot.ToWorldVec(), direction);

        return dotProduct < 0;
    }

    private static Vector2 GetRayPos(Vector2 xrayPos, Angle xrayRot)
    {
        return xrayPos + xrayRot.RotateVec(RayOffset);
    }
}
