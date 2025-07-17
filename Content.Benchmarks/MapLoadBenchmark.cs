// SPDX-FileCopyrightText: 2022 Paul Ritter <ritter.paul1@googlemail.com>
// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 TemporalOroboros <TemporalOroboros@gmail.com>
// SPDX-FileCopyrightText: 2023 Visne <39844191+Visne@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 IProduceWidgets <107586145+IProduceWidgets@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <92357316+Piras314@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Spessmann <156740760+Spessmann@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 heree <79546981+hereelabs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 terezi <147006836+terezi-station@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 LaCumbiaDelCoronavirus <90893484+LaCumbiaDelCoronavirus@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 mkanke-real <mikekanke@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Content.IntegrationTests;
using Content.IntegrationTests.Pair;
using Content.Server.Maps;
using Robust.Shared;
using Robust.Shared.Analyzers;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Benchmarks;

[Virtual]
public class MapLoadBenchmark
{
    private TestPair _pair = default!;
    private MapLoaderSystem _mapLoader = default!;
    private SharedMapSystem _mapSys = default!;

    [GlobalSetup]
    public void Setup()
    {
        ProgramShared.PathOffset = "../../../../";
        PoolManager.Startup();

        _pair = PoolManager.GetServerClient().GetAwaiter().GetResult();
        var server = _pair.Server;

        Paths = server.ResolveDependency<IPrototypeManager>()
            .EnumeratePrototypes<GameMapPrototype>()
            .ToDictionary(x => x.ID, x => x.MapPath.ToString());

        _mapLoader = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<MapLoaderSystem>();
        _mapSys = server.ResolveDependency<IEntitySystemManager>().GetEntitySystem<SharedMapSystem>();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        await _pair.DisposeAsync();
        PoolManager.Shutdown();
    }

    public static readonly string[] MapsSource = { "Empty", "Satlern", "Box", "Bagel", "Dev", "CentComm", "Atlas", "Core", "TestTeg", "TestHFR", "Packed", "Origin", "Omega", "Cluster", "Reach", "Meta", "Marathon", "Europa", "MeteorArena", "Fland", "Oasis", "Cog", "FlandHighPop", "OasisHighPop", "OriginHighPop", "Barratry", "Kettle", "Divide", "Glacier", "Hot Springs", "roid_outpost", "DMVStation", "Convex" }; //Goobstation, readds maps

    [ParamsSource(nameof(MapsSource))]
    public string Map;

    public Dictionary<string, string> Paths;
    private MapId _mapId;

    [Benchmark]
    public async Task LoadMap()
    {
        var mapPath = new ResPath(Paths[Map]);
        var server = _pair.Server;
        await server.WaitPost(() =>
        {
            var success = _mapLoader.TryLoadMap(mapPath, out var map, out _);
            if (!success)
                throw new Exception("Map load failed");
            _mapId = map.Value.Comp.MapId;
        });
    }

    [IterationCleanup]
    public void IterationCleanup()
    {
        var server = _pair.Server;
        server.WaitPost(() => _mapSys.DeleteMap(_mapId))
            .Wait();
    }
}
