using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Traits.Unions;

[Prototype("unionElgibleDepartment")]
public sealed class UnionElgibleDepartmentPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField] public List<string> EligibleDepartments = [];
}