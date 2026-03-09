#nullable enable
using System.Linq;
using System.Numerics;
using Content.IntegrationTests.Pair;
using Content.Server._Funkystation.WizardFamiliar;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.IntegrationTests.Tests._Funkystation.WizardFamiliar;

[TestFixture]
public sealed class WizardFamiliarTest
{
    [Test]
    public async Task SummonCreatesFamiliarWithCorrectComponents()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            InLobby = false,
            DummyTicker = false,
        });

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var timing = server.ResolveDependency<IGameTiming>();
        var actionSys = entMan.System<SharedActionsSystem>();
        var factionSys = entMan.System<NpcFactionSystem>();

        EntityUid wizard = default;
        EntityUid? summonActionId = null;

        await server.WaitPost(() =>
        {
            wizard = entMan.SpawnEntity("MobHuman", testMap.GridCoords);
            summonActionId = actionSys.AddAction(wizard, "ActionSummonMiniDragon");
            Assert.That(summonActionId, Is.Not.Null);
            Assert.That(entMan.TryGetComponent(summonActionId!.Value, out InstantActionComponent? instantAction), Is.True);
            actionSys.SetUseDelay(summonActionId.Value, null);
            actionSys.PerformAction(wizard, null, summonActionId.Value, instantAction!, instantAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitAssertion(() =>
        {
            var familiars = entMan.EntityQuery<WizardFamiliarComponent>()
                .Where(c => c.Wizard == wizard)
                .Select(e => e.Owner)
                .ToList();
            Assert.That(familiars, Has.Count.EqualTo(1), "Exactly one familiar should exist");

            var familiar = familiars[0];
            Assert.That(entMan.GetComponent<MetaDataComponent>(familiar).EntityPrototype?.ID, Is.EqualTo("MobMiniDragonFamiliar"));

            var familiarComp = entMan.GetComponent<WizardFamiliarComponent>(familiar);
            Assert.That(familiarComp.Wizard, Is.EqualTo(wizard));

            Assert.That(factionSys.IsIgnored(new Entity<FactionExceptionComponent?>(familiar, null), wizard), Is.True, "Wizard should be in familiar's FactionException.Ignored");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task SecondSummonBlockedWhileFirstAlive()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            InLobby = false,
            DummyTicker = false,
        });

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var timing = server.ResolveDependency<IGameTiming>();
        var actionSys = entMan.System<SharedActionsSystem>();

        EntityUid wizard = default;
        EntityUid? summonActionId = null;

        await server.WaitPost(() =>
        {
            wizard = entMan.SpawnEntity("MobHuman", testMap.GridCoords);
            summonActionId = actionSys.AddAction(wizard, "ActionSummonMiniDragon");
            Assert.That(summonActionId, Is.Not.Null);
            Assert.That(entMan.TryGetComponent(summonActionId!.Value, out InstantActionComponent? instantAction), Is.True);
            actionSys.SetUseDelay(summonActionId.Value, null);
            actionSys.PerformAction(wizard, null, summonActionId.Value, instantAction!, instantAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitPost(() =>
        {
            Assert.That(entMan.TryGetComponent(summonActionId!.Value, out InstantActionComponent? instantAction), Is.True);
            actionSys.SetUseDelay(summonActionId.Value, null);
            actionSys.PerformAction(wizard, null, summonActionId.Value, instantAction!, instantAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitAssertion(() =>
        {
            var familiars = entMan.EntityQuery<WizardFamiliarComponent>()
                .Where(c => c.Wizard == wizard && !entMan.System<MobStateSystem>().IsDead(c.Owner))
                .Select(e => e.Owner)
                .ToList();
            Assert.That(familiars, Has.Count.EqualTo(1), "Still only one living familiar after second summon attempt");
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FamiliarDeathTriggersCooldown()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            InLobby = false,
            DummyTicker = false,
        });

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var timing = server.ResolveDependency<IGameTiming>();
        var actionSys = entMan.System<SharedActionsSystem>();
        var damageSys = entMan.System<DamageableSystem>();
        var mobStateSys = entMan.System<MobStateSystem>();
        var protoMan = server.ResolveDependency<IPrototypeManager>();

        EntityUid wizard = default;
        EntityUid? summonActionId = null;
        EntityUid familiar = default;

        await server.WaitPost(() =>
        {
            wizard = entMan.SpawnEntity("MobHuman", testMap.GridCoords);
            summonActionId = actionSys.AddAction(wizard, "ActionSummonMiniDragon");
            Assert.That(summonActionId, Is.Not.Null);
            Assert.That(entMan.TryGetComponent(summonActionId!.Value, out InstantActionComponent? instantAction), Is.True);
            actionSys.SetUseDelay(summonActionId.Value, null);
            actionSys.PerformAction(wizard, null, summonActionId.Value, instantAction!, instantAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitPost(() =>
        {
            familiar = entMan.EntityQuery<WizardFamiliarComponent>()
                .Where(c => c.Wizard == wizard)
                .Select(e => e.Owner)
                .First();
            var damage = new DamageSpecifier(protoMan.Index<DamageGroupPrototype>("Toxin"), FixedPoint2.New(100000));
            damageSys.TryChangeDamage(familiar, damage, true);
        });

        await server.WaitRunTicks(5);

        await server.WaitAssertion(() =>
        {
            Assert.That(mobStateSys.IsDead(familiar), Is.True);
            var action = entMan.GetComponent<InstantActionComponent>(summonActionId!.Value);
            Assert.That(action.Cooldown, Is.Not.Null, "Summon action should have cooldown after familiar death");
            Assert.That(action.Cooldown!.Value.End, Is.GreaterThan(timing.CurTime));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TeleportActionMovesFamiliarToWizard()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Dirty = true,
            InLobby = false,
            DummyTicker = false,
        });

        var server = pair.Server;
        var testMap = await pair.CreateTestMap();
        var entMan = server.ResolveDependency<IEntityManager>();
        var timing = server.ResolveDependency<IGameTiming>();
        var actionSys = entMan.System<SharedActionsSystem>();
        var transformSys = entMan.System<SharedTransformSystem>();

        EntityUid wizard = default;
        EntityUid familiar = default;
        EntityUid teleportActionId = default;

        await server.WaitPost(() =>
        {
            wizard = entMan.SpawnEntity("MobHuman", testMap.GridCoords);
            var summonActionId = actionSys.AddAction(wizard, "ActionSummonMiniDragon");
            Assert.That(summonActionId, Is.Not.Null);
            Assert.That(entMan.TryGetComponent(summonActionId!.Value, out InstantActionComponent? instantAction), Is.True);
            actionSys.SetUseDelay(summonActionId.Value, null);
            actionSys.PerformAction(wizard, null, summonActionId.Value, instantAction!, instantAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitPost(() =>
        {
            familiar = entMan.EntityQuery<WizardFamiliarComponent>()
                .Where(c => c.Wizard == wizard)
                .Select(e => e.Owner)
                .First();
            var familiarActions = entMan.GetComponent<ActionsComponent>(familiar);
            teleportActionId = familiarActions.Actions
                .FirstOrDefault(a =>
                    entMan.TryGetComponent(a, out MetaDataComponent? m) &&
                    m.EntityPrototype?.ID == "ActionMiniDragonTeleportToWizard");
            Assert.That(teleportActionId, Is.Not.EqualTo(EntityUid.Invalid), "Familiar should have teleport action");
        });

        await server.WaitPost(() =>
        {
            transformSys.SetCoordinates(wizard, testMap.GridCoords.Offset(new Vector2(5, 5)));
        });

        await server.WaitRunTicks(5);

        await server.WaitPost(() =>
        {
            Assert.That(entMan.TryGetComponent(teleportActionId, out InstantActionComponent? teleportAction), Is.True);
            actionSys.SetUseDelay(teleportActionId, null);
            actionSys.PerformAction(familiar, null, teleportActionId, teleportAction!, teleportAction!.Event, timing.CurTime);
        });

        await server.WaitRunTicks(5);

        await server.WaitAssertion(() =>
        {
            var wizardPos = entMan.GetComponent<TransformComponent>(wizard).Coordinates;
            var familiarPos = entMan.GetComponent<TransformComponent>(familiar).Coordinates;
            Assert.That(familiarPos.EntityId, Is.EqualTo(wizardPos.EntityId), "Familiar should be on same grid as wizard");
            Assert.That((familiarPos.Position - wizardPos.Position).Length(), Is.LessThan(1f), "Familiar should be near wizard after teleport");
        });

        await pair.CleanReturnAsync();
    }
}
