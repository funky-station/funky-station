// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Popups;
using Content.Shared.Construction.Components;
using Content.Shared.CCVar;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;
using Content.Server.Atmos.Components;

namespace Content.Server.Atmos.EntitySystems;

/// <summary>
/// This handles restricting device entities with DeviceRestrictOverlapComponent from stacking with any other devices.
/// </summary>
public sealed class DeviceRestrictOverlapSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    private readonly List<EntityUid> _anchoredEntities = new();
    private EntityQuery<DeviceRestrictOverlapComponent> _deviceRestrictOverlapQuery;
    private EntityQuery<BinaryDeviceRestrictOverlapComponent> _binaryDeviceRestrictOverlapQuery;


    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DeviceRestrictOverlapComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
        SubscribeLocalEvent<DeviceRestrictOverlapComponent, AnchorAttemptEvent>(OnAnchorAttempt);

        _deviceRestrictOverlapQuery = GetEntityQuery<DeviceRestrictOverlapComponent>();
        _binaryDeviceRestrictOverlapQuery = GetEntityQuery<BinaryDeviceRestrictOverlapComponent>();
    }

    private void OnAnchorStateChanged(Entity<DeviceRestrictOverlapComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        if (HasComp<AnchorableComponent>(ent) && CheckOverlap(ent))
        {
            _popup.PopupEntity(Loc.GetString("device-restrict-overlap-popup-blocked", ("device", ent.Owner)), ent);
            _xform.Unanchor(ent, Transform(ent));
        }
    }

    private void OnAnchorAttempt(Entity<DeviceRestrictOverlapComponent> ent, ref AnchorAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (CheckOverlap(ent))
        {
            _popup.PopupEntity(Loc.GetString("device-restrict-overlap-popup-blocked", ("device", ent.Owner)), ent, args.User);
            args.Cancel();
        }
    }

    [PublicAPI]
    public bool CheckOverlap(EntityUid uid)
    {
        return CheckOverlap((uid, Transform(uid)));
    }

    public bool CheckOverlap(Entity<TransformComponent> ent)
    {
        if (ent.Comp.GridUid is not { } grid || !TryComp<MapGridComponent>(grid, out var gridComp))
            return false;

        var indices = _map.TileIndicesFor(grid, gridComp, ent.Comp.Coordinates);
        _anchoredEntities.Clear();
        _anchoredEntities.AddRange(_map.GetAnchoredEntities((grid, gridComp), indices));

        foreach (var otherEnt in _anchoredEntities)
        {
            // this should never actually happen but just for safety
            if (otherEnt == ent.Owner)
                continue;

            // Block if any device with DeviceRestrictOverlapComponent or BinaryDeviceRestrictOverlapComponent is present
            if (_deviceRestrictOverlapQuery.HasComp(otherEnt) || _binaryDeviceRestrictOverlapQuery.HasComp(otherEnt))
                return true;
        }

        return false;
    }
}