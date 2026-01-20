using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Traits.Unions;

[RegisterComponent]
public sealed partial class StationUnionsComponent : Component
{
    [DataField]
    public List<StationUnion> Unions { get; private set; }
}

public struct StationUnion
{
    public string Name;
    public List<string> Departments;
    public Dictionary<EntityUid, bool> Members;
    public EntityUid Leader;
    public bool OnStrike;
}