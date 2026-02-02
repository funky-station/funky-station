// SPDX-FileCopyrightText: 2025 B_Kirill <153602297+B-Kirill@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Server.DeviceNetwork.Components;
using Content.Server.Power.Components;
using Content.Shared.Hands;
using Content.Shared.Inventory.Events;
using Content.Shared.SurveillanceCamera.Components;
using Robust.Shared.Containers;

namespace Content.Server.SurveillanceCamera;

public sealed class SurveillanceCameraMapSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SurveillanceCameraComponent, MoveEvent>(OnCameraMoved);
        SubscribeLocalEvent<SurveillanceCameraComponent, EntityUnpausedEvent>(OnCameraUnpaused);

        // _Funkystation Changes Start
        // Hide bodycams from map when equipped or stored in containers
        SubscribeLocalEvent<SurveillanceCameraComponent, GotEquippedEvent>(OnCameraContained);
        SubscribeLocalEvent<SurveillanceCameraComponent, GotEquippedHandEvent>(OnCameraContained);
        SubscribeLocalEvent<SurveillanceCameraComponent, EntInsertedIntoContainerMessage>(OnCameraContained);
        // Show bodycams on map when unequipped or removed from containers
        SubscribeLocalEvent<SurveillanceCameraComponent, GotUnequippedEvent>(OnCameraFreed);
        SubscribeLocalEvent<SurveillanceCameraComponent, GotUnequippedHandEvent>(OnCameraFreed);
        SubscribeLocalEvent<SurveillanceCameraComponent, EntRemovedFromContainerMessage>(OnCameraFreed);
        // _Funkystation Changes End

        SubscribeNetworkEvent<RequestCameraMarkerUpdateMessage>(OnRequestCameraMarkerUpdate);
    }

    // _Funkystation Changes Start
    // This prevents bodycams from being visible on the map.
    // Having them visible would provide a rather wild benefit to effectively being a second set of coords.
    // So instead of making people have to search someone for a bodycam hidden in their bag or pocket
    // We just hide it entirely from the map, because fuck you, you are going to die in maints alone.
    private void OnCameraContained(EntityUid uid, SurveillanceCameraComponent comp, EntityEventArgs args)
    {
        // Hide camera from map when equipped or put in any container
        SetCameraVisibility(uid, false);
    }

    private void OnCameraFreed(EntityUid uid, SurveillanceCameraComponent comp, EntityEventArgs args)
    {
        // Show camera on map when unequipped or removed from container
        SetCameraVisibility(uid, true);
        UpdateCameraMarker((uid, comp));
    }
    // _Funkystation Changes End

    private void OnCameraUnpaused(EntityUid uid, SurveillanceCameraComponent comp, ref EntityUnpausedEvent args)
    {
        if (Terminating(uid))
            return;

        UpdateCameraMarker((uid, comp));
    }

    private void OnCameraMoved(EntityUid uid, SurveillanceCameraComponent comp, ref MoveEvent args)
    {
        if (Terminating(uid))
            return;

        var oldGridUid = _transform.GetGrid(args.OldPosition);
        var newGridUid = _transform.GetGrid(args.NewPosition);

        if (oldGridUid != newGridUid && oldGridUid is not null && !Terminating(oldGridUid.Value))
        {
            if (TryComp<SurveillanceCameraMapComponent>(oldGridUid, out var oldMapComp))
            {
                var netEntity = GetNetEntity(uid);
                if (oldMapComp.Cameras.Remove(netEntity))
                    Dirty(oldGridUid.Value, oldMapComp);
            }
        }

        if (newGridUid is not null && !Terminating(newGridUid.Value))
            UpdateCameraMarker((uid, comp));
    }

    private void OnRequestCameraMarkerUpdate(RequestCameraMarkerUpdateMessage args)
    {
        var cameraEntity = GetEntity(args.CameraEntity);

        if (TryComp<SurveillanceCameraComponent>(cameraEntity, out var comp)
            && HasComp<DeviceNetworkComponent>(cameraEntity))
            UpdateCameraMarker((cameraEntity, comp));
    }

    /// <summary>
    /// Updates camera data in the SurveillanceCameraMapComponent for the specified camera entity.
    /// </summary>
    public void UpdateCameraMarker(Entity<SurveillanceCameraComponent> camera)
    {
        var (uid, comp) = camera;

        if (Terminating(uid))
            return;

        if (!TryComp(uid, out TransformComponent? xform) || !TryComp(uid, out DeviceNetworkComponent? deviceNet))
            return;

        var gridUid = xform.GridUid ?? xform.MapUid;
        if (gridUid is null)
            return;

        var netEntity = GetNetEntity(uid);

        var mapComp = EnsureComp<SurveillanceCameraMapComponent>(gridUid.Value);
        var worldPos = _transform.GetWorldPosition(xform);
        var gridMatrix = _transform.GetInvWorldMatrix(Transform(gridUid.Value));
        var localPos = Vector2.Transform(worldPos, gridMatrix);

        var address = deviceNet.Address;
        var subnet = deviceNet.ReceiveFrequencyId ?? string.Empty;
        var powered = CompOrNull<ApcPowerReceiverComponent>(uid)?.Powered ?? true;
        var active = comp.Active && powered;

        bool exists = mapComp.Cameras.TryGetValue(netEntity, out var existing);

        if (exists &&
            existing.Position.Equals(localPos) &&
            existing.Active == active &&
            existing.Address == address &&
            existing.Subnet == subnet)
        {
            return;
        }

        var visible = exists ? existing.Visible : true;

        mapComp.Cameras[netEntity] = new CameraMarker
        {
            Position = localPos,
            Active = active,
            Address = address,
            Subnet = subnet,
            Visible = visible
        };
        Dirty(gridUid.Value, mapComp);
    }

    /// <summary>
    /// Sets the visibility state of a camera on the camera map.
    /// </summary>
    public void SetCameraVisibility(EntityUid cameraUid, bool visible)
    {
        if (!TryComp(cameraUid, out TransformComponent? xform))
            return;

        var gridUid = xform.GridUid ?? xform.MapUid;
        if (gridUid == null || !TryComp<SurveillanceCameraMapComponent>(gridUid.Value, out var mapComp))
            return;

        var netEntity = GetNetEntity(cameraUid);
        if (mapComp.Cameras.TryGetValue(netEntity, out var marker))
        {
            marker.Visible = visible;
            mapComp.Cameras[netEntity] = marker;
            Dirty(gridUid.Value, mapComp);
        }
    }

    /// <summary>
    /// Checks if a camera is currently visible on the camera map.
    /// </summary>
    public bool IsCameraVisible(EntityUid cameraUid)
    {
        if (!TryComp(cameraUid, out TransformComponent? xform))
            return false;

        var gridUid = xform.GridUid ?? xform.MapUid;
        if (gridUid == null || !TryComp<SurveillanceCameraMapComponent>(gridUid, out var mapComp))
            return false;

        var netEntity = GetNetEntity(cameraUid);
        return mapComp.Cameras.TryGetValue(netEntity, out var marker) && marker.Visible;
    }
}
