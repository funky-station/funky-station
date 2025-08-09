// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Ilya246 <57039557+Ilya246@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 router <messagebus@vk.com>
// SPDX-FileCopyrightText: 2025 ArtisticRoomba <145879011+ArtisticRoomba@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.NodeContainer;
using Content.Server.NodeContainer.Nodes;
using Content.Server.Popups;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.CCVar;
using Content.Shared.Construction.Components;
using Content.Shared.NodeContainer;
using JetBrains.Annotations;
using Robust.Server.GameObjects;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Server.Atmos.EntitySystems;

/// <summary>
/// This handles restricting binary device entities from overlapping with other binary devices or devices with DeviceRestrictOverlapComponent.
/// </summary>
public sealed class BinaryDeviceRestrictOverlapSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    private readonly List<EntityUid> _anchoredEntities = new();
    private EntityQuery<NodeContainerComponent> _nodeContainerQuery;
    private EntityQuery<BinaryDeviceRestrictOverlapComponent> _binaryRestrictOverlapQuery;
    private EntityQuery<DeviceRestrictOverlapComponent> _deviceRestrictOverlapQuery;

    public bool StrictDeviceStacking = false;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<BinaryDeviceRestrictOverlapComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
        SubscribeLocalEvent<BinaryDeviceRestrictOverlapComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        //Subs.CVar(_cfg, CCVars.StrictDeviceStacking, (bool val) => { StrictDeviceStacking = val; }, false);

        _nodeContainerQuery = GetEntityQuery<NodeContainerComponent>();
        _binaryRestrictOverlapQuery = GetEntityQuery<BinaryDeviceRestrictOverlapComponent>();
        _deviceRestrictOverlapQuery = GetEntityQuery<DeviceRestrictOverlapComponent>();
    }

    private void OnAnchorStateChanged(Entity<BinaryDeviceRestrictOverlapComponent> ent, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
            return;

        if (HasComp<AnchorableComponent>(ent) && CheckOverlap(ent))
        {
            _popup.PopupEntity(Loc.GetString("device-restrict-overlap-popup-blocked", ("device", ent.Owner)), ent);
            _xform.Unanchor(ent, Transform(ent));
        }
    }

    private void OnAnchorAttempt(Entity<BinaryDeviceRestrictOverlapComponent> ent, ref AnchorAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (!_nodeContainerQuery.TryComp(ent, out var node))
            return;

        var xform = Transform(ent);
        if (CheckOverlap((ent, node, xform)))
        {
            _popup.PopupEntity(Loc.GetString("device-restrict-overlap-popup-blocked", ("device", ent.Owner)), ent, args.User);
            args.Cancel();
        }
    }

    [PublicAPI]
    public bool CheckOverlap(EntityUid uid)
    {
        if (!_nodeContainerQuery.TryComp(uid, out var node))
            return false;

        return CheckOverlap((uid, node, Transform(uid)));
    }

    public bool CheckOverlap(Entity<NodeContainerComponent, TransformComponent> ent)
    {
        if (ent.Comp2.GridUid is not { } grid || !TryComp<MapGridComponent>(grid, out var gridComp))
            return false;

        var indices = _map.TileIndicesFor(grid, gridComp, ent.Comp2.Coordinates);
        _anchoredEntities.Clear();
        _map.GetAnchoredEntities((grid, gridComp), indices, _anchoredEntities);

        // ATMOS: change to long if you add more pipe layers than 5 + z levels
        var takenDirs = PipeDirection.None;

        foreach (var otherEnt in _anchoredEntities)
        {
            // this should never actually happen but just for safety
            if (otherEnt == ent.Owner)
                continue;

            if (!_nodeContainerQuery.TryComp(otherEnt, out var otherComp))
                continue;

            // Check for DeviceRestrictOverlapComponent first, block immediately if present
            if (_deviceRestrictOverlapQuery.HasComp(otherEnt))
                return true;

            // Only check binary devices with BinaryDeviceRestrictOverlapComponent
            if (!_binaryRestrictOverlapQuery.HasComp(otherEnt))
                continue;

            var (overlapping, which) = BinaryDeviceNodesOverlap(ent, (otherEnt, otherComp, Transform(otherEnt)), takenDirs);
            takenDirs |= which;

            if (overlapping)
                return true;
        }

        return false;
    }

    public (bool, PipeDirection) BinaryDeviceNodesOverlap(Entity<NodeContainerComponent, TransformComponent> ent, Entity<NodeContainerComponent, TransformComponent> other, PipeDirection takenDirs)
    {
        var entDirsAndLayers = GetAllDirectionsAndLayers(ent).ToList();
        var otherDirsAndLayers = GetAllDirectionsAndLayers(other).ToList();
        var entDirsCollapsed = PipeDirection.None;

        foreach (var (dir, layer) in entDirsAndLayers)
        {
            entDirsCollapsed |= dir;
            foreach (var (otherDir, otherLayer) in otherDirsAndLayers)
            {
                if (layer != otherLayer)
                    continue;

                takenDirs |= otherDir;
                if (StrictDeviceStacking)
                    if ((dir & otherDir) != 0)
                        return (true, takenDirs);
                else
                    if ((dir ^ otherDir) != 0)
                        break;
            }
        }

        // If no strict binary device stacking, then output ("are all entDirs occupied", takenDirs)

        return (StrictDeviceStacking ? false : ((takenDirs & entDirsCollapsed) == entDirsCollapsed), takenDirs);

        IEnumerable<(PipeDirection, AtmosPipeLayer)> GetAllDirectionsAndLayers(Entity<NodeContainerComponent, TransformComponent> pipe)
        {
            foreach (var node in pipe.Comp1.Nodes.Values)
            {
                // we need to rotate the pipe manually like this because the rotation doesn't update for pipes that are unanchored.
                if (node is PipeNode pipeNode)
                    yield return (pipeNode.OriginalPipeDirection.RotatePipeDirection(pipe.Comp2.LocalRotation), pipeNode.CurrentPipeLayer);
            }
        }
    }
}