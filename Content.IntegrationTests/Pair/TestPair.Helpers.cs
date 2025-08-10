// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 ElectroJr <leonsfriedrich@gmail.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Quantum-cross <7065792+Quantum-cross@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

#nullable enable
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Database;
using Content.Server.Preferences.Managers;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.UnitTesting;

namespace Content.IntegrationTests.Pair;

// Contains misc helper functions to make writing tests easier.
public sealed partial class TestPair
{
    /// <summary>
    /// Creates a map, a grid, and a tile, and gives back references to them.
    /// </summary>
    [MemberNotNull(nameof(TestMap))]
    public async Task<TestMapData> CreateTestMap(bool initialized = true, string tile = "Plating")
    {
        var mapData = new TestMapData();
        TestMap = mapData;
        await Server.WaitIdleAsync();
        var tileDefinitionManager = Server.ResolveDependency<ITileDefinitionManager>();

        TestMap = mapData;
        await Server.WaitPost(() =>
        {
            mapData.MapUid = Server.System<SharedMapSystem>().CreateMap(out mapData.MapId, runMapInit: initialized);
            mapData.Grid = Server.MapMan.CreateGridEntity(mapData.MapId);
            mapData.GridCoords = new EntityCoordinates(mapData.Grid, 0, 0);
            var plating = tileDefinitionManager[tile];
            var platingTile = new Tile(plating.TileId);
            Server.System<SharedMapSystem>().SetTile(mapData.Grid.Owner, mapData.Grid.Comp, mapData.GridCoords, platingTile);
            mapData.MapCoords = new MapCoordinates(0, 0, mapData.MapId);
            mapData.Tile = Server.System<SharedMapSystem>().GetAllTiles(mapData.Grid.Owner, mapData.Grid.Comp).First();
        });

        TestMap = mapData;
        if (!Settings.Connected)
            return mapData;

        await RunTicksSync(10);
        mapData.CMapUid = ToClientUid(mapData.MapUid);
        mapData.CGridUid = ToClientUid(mapData.Grid);
        mapData.CGridCoords = new EntityCoordinates(mapData.CGridUid, 0, 0);

        TestMap = mapData;
        return mapData;
    }

    /// <summary>
    /// Convert a client-side uid into a server-side uid
    /// </summary>
    public EntityUid ToServerUid(EntityUid uid) => ConvertUid(uid, Client, Server);

    /// <summary>
    /// Convert a server-side uid into a client-side uid
    /// </summary>
    public EntityUid ToClientUid(EntityUid uid) => ConvertUid(uid, Server, Client);

    private static EntityUid ConvertUid(
        EntityUid uid,
        RobustIntegrationTest.IntegrationInstance source,
        RobustIntegrationTest.IntegrationInstance destination)
    {
        if (!uid.IsValid())
            return EntityUid.Invalid;

        if (!source.EntMan.TryGetComponent<MetaDataComponent>(uid, out var meta))
        {
            Assert.Fail($"Failed to resolve MetaData while converting the EntityUid for entity {uid}");
            return EntityUid.Invalid;
        }

        if (!destination.EntMan.TryGetEntity(meta.NetEntity, out var otherUid))
        {
            Assert.Fail($"Failed to resolve net ID while converting the EntityUid entity {source.EntMan.ToPrettyString(uid)}");
            return EntityUid.Invalid;
        }

        return otherUid.Value;
    }

    /// <summary>
    /// Execute a command on the server and wait some number of ticks.
    /// </summary>
    public async Task WaitCommand(string cmd, int numTicks = 10)
    {
        await Server.ExecuteCommand(cmd);
        await RunTicksSync(numTicks);
    }

    /// <summary>
    /// Execute a command on the client and wait some number of ticks.
    /// </summary>
    public async Task WaitClientCommand(string cmd, int numTicks = 10)
    {
        await Client.ExecuteCommand(cmd);
        await RunTicksSync(numTicks);
    }

    /// <summary>
    /// Retrieve all entity prototypes that have some component.
    /// </summary>
    public List<(EntityPrototype, T)> GetPrototypesWithComponent<T>(
        HashSet<string>? ignored = null,
        bool ignoreAbstract = true,
        bool ignoreTestPrototypes = true)
        where T : IComponent, new()
    {
        if (!Server.ResolveDependency<IComponentFactory>().TryGetRegistration<T>(out var reg)
            && !Client.ResolveDependency<IComponentFactory>().TryGetRegistration<T>(out reg))
        {
            Assert.Fail($"Unknown component: {typeof(T).Name}");
            return new();
        }

        var id = reg.Name;
        var list = new List<(EntityPrototype, T)>();
        foreach (var proto in Server.ProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (ignored != null && ignored.Contains(proto.ID))
                continue;

            if (ignoreAbstract && proto.Abstract)
                continue;

            if (ignoreTestPrototypes && IsTestPrototype(proto))
                continue;

            if (proto.Components.TryGetComponent(id, out var cmp))
                list.Add((proto, (T)cmp));
        }

        return list;
    }

    /// <summary>
    /// Retrieve all entity prototypes that have some component.
    /// </summary>
    public List<EntityPrototype> GetPrototypesWithComponent(Type type,
        HashSet<string>? ignored = null,
        bool ignoreAbstract = true,
        bool ignoreTestPrototypes = true)
    {
        var id = Server.ResolveDependency<IComponentFactory>().GetComponentName(type);
        var list = new List<EntityPrototype>();
        foreach (var proto in Server.ProtoMan.EnumeratePrototypes<EntityPrototype>())
        {
            if (ignored != null && ignored.Contains(proto.ID))
                continue;

            if (ignoreAbstract && proto.Abstract)
                continue;

            if (ignoreTestPrototypes && IsTestPrototype(proto))
                continue;

            if (proto.Components.ContainsKey(id))
                list.Add((proto));
        }

        return list;
    }

    /// <summary>
    /// Add dummy players to the pair with server saved job priority preferences
    /// </summary>
    /// <param name="jobPriorities">Job priorities to initialize the players with</param>
    /// <param name="count">How many players to add</param>
    /// <returns>Enumerable of sessions for the new players</returns>
    [PublicAPI]
    public Task<IEnumerable<ICommonSession>> AddDummyPlayers(Dictionary<ProtoId<JobPrototype>,JobPriority> jobPriorities, int count=1)
    {
        return AddDummyPlayers(jobPriorities, jobPriorities.Keys, count);
    }

    [PublicAPI]
    public async Task<IEnumerable<ICommonSession>> AddDummyPlayers(
        Dictionary<ProtoId<JobPrototype>,JobPriority> jobPriorities,
        IEnumerable<ProtoId<JobPrototype>> jobPreferences,
        int count=1)
    {
        var prefMan = Server.ResolveDependency<IServerPreferencesManager>();
        var dbMan = Server.ResolveDependency<UserDbDataManager>();

        var sessions = await Server.AddDummySessions(count);
        await RunTicksSync(5);
        var tasks = sessions.Select(s =>
        {
            // dbMan.ClientConnected(s);
            dbMan.WaitLoadComplete(s).Wait();
            var newProfile = HumanoidCharacterProfile.Random().WithJobPreferences(jobPreferences).AsEnabled();
            return Task.WhenAll(
                prefMan.SetJobPriorities(s.UserId, jobPriorities),
                prefMan.SetProfile(s.UserId, 0, newProfile));
        });
        await Server.WaitPost(() => Task.WhenAll(tasks).Wait());
        await RunTicksSync(5);

        return sessions;
    }
}
