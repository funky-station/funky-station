using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

[Prototype("jobAlternateTitle")]
public sealed partial class JobAlternateTitlePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public string Name = default!;

    public string LocalizedName => Loc.GetString(Name);

    [DataField]
    public HashSet<JobRequirement>? Requirements;
}
