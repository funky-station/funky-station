using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Funkystation;

[TestFixture]
public sealed class MalfAiProtosExist
{
    public static readonly string[] ProtoIds =
    {
        "ActionMalfAiRoboticsFactory",
        "RoboticsFactoryGrid",
    };

    [Test]
    [TestCaseSource(nameof(ProtoIds))]
    public async Task MalfAiProtoExist(string protoId)
    {
        await using var pair = await PoolManager.GetServerClient();
        var protoMan = pair.Server.ResolveDependency<IPrototypeManager>();

        Assert.That(protoMan.HasIndex(protoId), Is.True, $"Missing prototype: {protoId}");
    }
}
