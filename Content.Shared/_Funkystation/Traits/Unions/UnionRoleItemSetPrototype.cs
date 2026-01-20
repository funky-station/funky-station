using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Traits.Unions;

[Prototype("unionLeaderItemSet")]
public sealed class UnionRoleItemSetPrototype : IPrototype 
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)] 
    public List<EntProtoId> Items = [];
}