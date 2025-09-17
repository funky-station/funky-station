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
        await pair.Client.WaitIdleAsync();
        await pair.Server.WaitIdleAsync();

        var clientProto = pair.Client.ResolveDependency<IPrototypeManager>();
        var serverProto = pair.Server.ResolveDependency<IPrototypeManager>();

        var has = clientProto.HasIndex(protoId) || serverProto.HasIndex(protoId);
        Assert.That(has, Is.True, $"Missing prototype: {protoId}");
    }
}
