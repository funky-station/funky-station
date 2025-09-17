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

        // Dynamically discover AI build prototypes by examining systems that use typed prototype IDs
        var buildPrototypes = new HashSet<EntProtoId>();

        // Get prototypes from MalfAiRoboticsFactorySystem by scanning all static non-public EntProtoId fields
        var factorySystemType = typeof(MalfAiRoboticsFactorySystem);
        foreach (var field in factorySystemType.GetFields(BindingFlags.NonPublic | BindingFlags.Static))
        {
            if (field.FieldType == typeof(EntProtoId))
            {
                var value = field.GetValue(null);
                if (value is EntProtoId entId)
                    buildPrototypes.Add(entId);
            }
        }

        Assert.That(buildPrototypes.Count, Is.GreaterThan(0), "No AI build prototypes were discovered");

        foreach (var prototype in buildPrototypes)
        {
            Assert.That(protoMan.HasIndex<EntityPrototype>(prototype.Id), Is.True,
                $"AI build prototype validation failed: Missing prototype '{prototype.Id}'. " +
                $"This prototype is referenced by AI build systems but doesn't exist.");
        }
    }
}
