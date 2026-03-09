// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.ResourceOverview.BUI;

[Serializable, NetSerializable]
public sealed class ResourceOverviewEntry
{
    public NetEntity NetEntity;
    public string DisplayName;
    public NetCoordinates Coordinates;
    public Dictionary<string, int> Materials;
    public bool IsSilo;
    public bool IsLowStock;
    /// <summary>
    /// Name of the nearest station beacon to this entry, or null if none.
    /// </summary>
    public string? NearestBeaconName;
    /// <summary>
    /// For lathes: the silo they are linked to, if any.
    /// </summary>
    public NetEntity? LinkedSiloNetEntity;

    public ResourceOverviewEntry(NetEntity netEntity, string displayName, NetCoordinates coordinates,
        Dictionary<string, int> materials, bool isSilo, bool isLowStock,
        string? nearestBeaconName = null, NetEntity? linkedSiloNetEntity = null)
    {
        NetEntity = netEntity;
        DisplayName = displayName;
        Coordinates = coordinates;
        Materials = materials;
        IsSilo = isSilo;
        IsLowStock = isLowStock;
        NearestBeaconName = nearestBeaconName;
        LinkedSiloNetEntity = linkedSiloNetEntity;
    }
}

[Serializable, NetSerializable]
public sealed class ResourceOverviewConnection
{
    public NetEntity LatheNetEntity;
    public NetEntity SiloNetEntity;
    public NetCoordinates LatheCoords;
    public NetCoordinates SiloCoords;

    public ResourceOverviewConnection(NetEntity latheNetEntity, NetEntity siloNetEntity, NetCoordinates latheCoords, NetCoordinates siloCoords)
    {
        LatheNetEntity = latheNetEntity;
        SiloNetEntity = siloNetEntity;
        LatheCoords = latheCoords;
        SiloCoords = siloCoords;
    }
}

[Serializable, NetSerializable]
public sealed class ResourceOverviewConsoleBoundInterfaceState : BoundUserInterfaceState
{
    public ResourceOverviewEntry[] Silos;
    public ResourceOverviewEntry[] Lathes;
    public ResourceOverviewConnection[] Connections;
    public bool HasAlerts;

    public ResourceOverviewConsoleBoundInterfaceState(
        ResourceOverviewEntry[] silos,
        ResourceOverviewEntry[] lathes,
        ResourceOverviewConnection[] connections,
        bool hasAlerts)
    {
        Silos = silos;
        Lathes = lathes;
        Connections = connections;
        HasAlerts = hasAlerts;
    }
}
