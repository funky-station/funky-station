using System.Collections.Generic;
using System.Reflection;
using Content.Server._Funkystation.MalfAI;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Funkystation;

[TestFixture]
public sealed class AiBuildPrototypeValidationTest
{
    [Test]
    public async Task AllAiBuildPrototypesExist()
    {
        await using var pair = await PoolManager.GetServerClient();
        var protoMan = pair.Server.ResolveDependency<IPrototypeManager>();

        // Dynamically discover all AI build prototypes by examining systems that use them
        var buildPrototypes = new HashSet<string>();

        // Get prototypes from MalfAiRoboticsFactorySystem
        var factorySystemType = typeof(MalfAiRoboticsFactorySystem);
        var roboticsFactoryPrototypeField = factorySystemType.GetField("RoboticsFactoryPrototype",
            BindingFlags.NonPublic | BindingFlags.Static);

        if (roboticsFactoryPrototypeField != null)
        {
            var prototypeValue = roboticsFactoryPrototypeField.GetValue(null) as string;
            if (!string.IsNullOrEmpty(prototypeValue))
                buildPrototypes.Add(prototypeValue);
        }

        // Test each discovered prototype using the same validation logic as lines 133-138 in AiBuildActionSystem.cs:
        // if (string.IsNullOrWhiteSpace(args.Prototype) || !_prototypes.HasIndex<EntityPrototype>(args.Prototype))
        Assert.That(buildPrototypes.Count, Is.GreaterThan(0), "No AI build prototypes were discovered");

        foreach (var prototype in buildPrototypes)
        {
            Assert.That(protoMan.HasIndex<EntityPrototype>(prototype), Is.True,
                $"AI build prototype validation failed: Missing prototype '{prototype}'. " +
                $"This prototype is used by AI build systems but doesn't exist.");
        }
    }
}
