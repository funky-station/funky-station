using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Funkystation;

[TestFixture]
public sealed class AIShaderProtosExist
{
    public static readonly string[] ShaderProtoIds =
    {
        "CameraStatic",
        "StencilMask",
        "StencilDraw",
    };

    [Test]
    [TestCaseSource(nameof(ShaderProtoIds))]
    public async Task ShaderProtoExists(string protoId)
    {
        await using var pair = await PoolManager.GetServerClient();
        var protoMan = pair.Server.ResolveDependency<IPrototypeManager>();

        Assert.That(protoMan.HasIndex(protoId), Is.True, $"Missing shader prototype: {protoId}");
    }
}
