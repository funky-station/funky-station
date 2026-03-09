// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Content.Shared.Materials.MaterialSilo;
using Content.Shared.Pinpointer;
using Content.Shared._Funkystation.ResourceOverview.BUI;
using Content.Shared._Funkystation.ResourceOverview.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;

namespace Content.Server._Funkystation.ResourceOverview;

public sealed class ResourceOverviewConsoleSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly SharedMaterialStorageSystem _materialStorage = default!;

    private const float UpdateTime = 1.0f;
    private float _updateTimer;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ResourceOverviewConsoleComponent, BoundUIOpenedEvent>(OnBoundUIOpened);
    }

    private void OnBoundUIOpened(Entity<ResourceOverviewConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUIState(ent.Owner, ent.Comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateTimer += frameTime;

        if (_updateTimer >= UpdateTime)
        {
            _updateTimer -= UpdateTime;

            var query = AllEntityQuery<ResourceOverviewConsoleComponent>();
            while (query.MoveNext(out var uid, out var component))
            {
                if (!_userInterfaceSystem.IsUiOpen(uid, ResourceOverviewConsoleUiKey.Key))
                    continue;

                UpdateUIState(uid, component);
            }
        }
    }

    private void UpdateUIState(EntityUid uid, ResourceOverviewConsoleComponent component)
    {
        var consoleXform = Transform(uid);

        if (consoleXform.GridUid == null)
            return;

        var gridUid = consoleXform.GridUid.Value;

        if (!HasComp<MapGridComponent>(gridUid))
            return;

        var navMap = EnsureComp<NavMapComponent>(gridUid);

        var silos = new List<ResourceOverviewEntry>();
        var lathes = new List<ResourceOverviewEntry>();
        var connections = new List<ResourceOverviewConnection>();
        var hasAlerts = false;

        var siloQuery = AllEntityQuery<MaterialSiloComponent, MaterialStorageComponent, TransformComponent>();
        while (siloQuery.MoveNext(out var siloUid, out _, out var storage, out var xform))
        {
            if (xform.GridUid != gridUid || !xform.Anchored)
                continue;

            var materials = _materialStorage.GetStoredMaterials((siloUid, storage), localOnly: true);
            var materialsDict = new Dictionary<string, int>();
            foreach (var (mat, amount) in materials)
            {
                materialsDict[mat.ToString()] = amount;
            }

            var isLowStock = IsLowStock(materialsDict, component);
            if (isLowStock)
                hasAlerts = true;

            var nearestBeacon = GetNearestBeaconName(navMap, xform.LocalPosition);

            silos.Add(new ResourceOverviewEntry(
                GetNetEntity(siloUid),
                Name(siloUid),
                GetNetCoordinates(xform.Coordinates),
                materialsDict,
                isSilo: true,
                isLowStock,
                nearestBeaconName: nearestBeacon));
        }

        var latheQuery = AllEntityQuery<LatheComponent, MaterialStorageComponent, TransformComponent>();
        while (latheQuery.MoveNext(out var latheUid, out _, out var storage, out var xform))
        {
            if (xform.GridUid != gridUid || !xform.Anchored)
                continue;

            var materials = _materialStorage.GetStoredMaterials((latheUid, storage), localOnly: false);
            var materialsDict = new Dictionary<string, int>();
            foreach (var (mat, amount) in materials)
            {
                materialsDict[mat.ToString()] = amount;
            }

            var isLowStock = IsLowStock(materialsDict, component);
            if (isLowStock)
                hasAlerts = true;

            var latheCoords = GetNetCoordinates(xform.Coordinates);
            NetEntity? linkedSilo = null;

            if (TryComp<MaterialSiloClientComponent>(latheUid, out var client) && client.Silo is { } siloUid)
            {
                if (Exists(siloUid) && Transform(siloUid).GridUid == gridUid)
                {
                    var siloCoords = GetNetCoordinates(Transform(siloUid).Coordinates);
                    connections.Add(new ResourceOverviewConnection(
                        GetNetEntity(latheUid),
                        GetNetEntity(siloUid),
                        latheCoords,
                        siloCoords));
                    linkedSilo = GetNetEntity(siloUid);
                }
            }

            var nearestBeacon = GetNearestBeaconName(navMap, xform.LocalPosition);

            lathes.Add(new ResourceOverviewEntry(
                GetNetEntity(latheUid),
                Name(latheUid),
                latheCoords,
                materialsDict,
                isSilo: false,
                isLowStock,
                nearestBeaconName: nearestBeacon,
                linkedSiloNetEntity: linkedSilo));
        }

        var state = new ResourceOverviewConsoleBoundInterfaceState(
            silos.ToArray(),
            lathes.ToArray(),
            connections.ToArray(),
            hasAlerts);

        _userInterfaceSystem.SetUiState(uid, ResourceOverviewConsoleUiKey.Key, state);
    }

    private bool IsLowStock(Dictionary<string, int> materials, ResourceOverviewConsoleComponent component)
    {
        foreach (var essential in component.EssentialMaterials)
        {
            var amount = materials.GetValueOrDefault(essential.ToString(), 0);
            if (amount < component.LowMaterialThreshold)
                return true;
        }

        return false;
    }

    private static string? GetNearestBeaconName(NavMapComponent navMap, Vector2 localPos)
    {
        string? nearestName = null;
        var nearestDistSq = float.PositiveInfinity;

        foreach (var (_, beacon) in navMap.Beacons)
        {
            var diff = beacon.Position - localPos;
            var distSq = diff.LengthSquared();
            if (distSq < nearestDistSq)
            {
                nearestDistSq = distSq;
                nearestName = beacon.Text;
            }
        }

        return nearestName;
    }
}
