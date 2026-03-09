// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Materials;
using Content.Server._Funkystation.ResourceOverview;
using Content.Shared.Materials;
using Content.Shared._Funkystation.ResourceOverview.BUI;
using Content.Shared._Funkystation.ResourceOverview.Components;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.IntegrationTests.Tests._Funkystation.ResourceOverview;

[TestFixture]
[TestOf(typeof(ResourceOverviewConsoleSystem))]
public sealed class ResourceOverviewConsoleTest
{
    [Test]
    public async Task ResourceOverviewConsole_ShowsSilosAndLathes()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;
        var entMan = server.ResolveDependency<IEntityManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var uiSystem = server.System<UserInterfaceSystem>();
        var materialStorage = server.System<SharedMaterialStorageSystem>();

        var testMap = await pair.CreateTestMap();
        var coords = testMap.GridCoords;

        EntityUid console = default;
        EntityUid silo = default;
        EntityUid lathe = default;

        await server.WaitAssertion(() =>
        {
            console = entMan.SpawnEntity("ComputerResourceOverview", coords);
            silo = entMan.SpawnEntity("MachineMaterialSilo", coords);
            lathe = entMan.SpawnEntity("Autolathe", coords);

            Assert.That(entMan.HasComponent<ResourceOverviewConsoleComponent>(console));
            Assert.That(entMan.HasComponent<MaterialStorageComponent>(silo));
            Assert.That(entMan.HasComponent<MaterialStorageComponent>(lathe));

            materialStorage.TryChangeMaterialAmount(silo, "Steel", 50);
            materialStorage.TryChangeMaterialAmount(silo, "Glass", 30);
            materialStorage.TryChangeMaterialAmount(silo, "Plastic", 20);
        });

        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var player = pair.Player;
            Assert.That(player, Is.Not.Null);
            Assert.That(player!.AttachedEntity, Is.Not.Null);
            Assert.That(uiSystem.TryOpenUi(console, ResourceOverviewConsoleUiKey.Key, player.AttachedEntity!.Value));
        });

        await pair.RunTicksSync(10);

        await server.WaitAssertion(() =>
        {
            Assert.That(uiSystem.TryGetUiState<ResourceOverviewConsoleBoundInterfaceState>(
                console, ResourceOverviewConsoleUiKey.Key, out var resourceState));
            Assert.That(resourceState, Is.Not.Null);

            Assert.That(resourceState!.Silos.Length, Is.GreaterThanOrEqualTo(1));
            Assert.That(resourceState.Lathes.Length, Is.GreaterThanOrEqualTo(1));

            var siloEntry = resourceState.Silos.FirstOrDefault(s => entMan.GetEntity(s.NetEntity) == silo);
            Assert.That(siloEntry, Is.Not.Null);
            Assert.That(siloEntry!.Materials.TryGetValue("Steel", out var steelVal) ? steelVal : 0, Is.EqualTo(50));
            Assert.That(siloEntry.Materials.TryGetValue("Glass", out var glassVal) ? glassVal : 0, Is.EqualTo(30));
            Assert.That(siloEntry.Materials.TryGetValue("Plastic", out var plasticVal) ? plasticVal : 0, Is.EqualTo(20));

            mapSystem.DeleteMap(testMap.MapId);
        });

        await pair.CleanReturnAsync();
    }
}
