using Content.Server.Objectives.Systems;

namespace Content.Server.Funkystation.Objectives.Components;

[RegisterComponent, Access(typeof(KeepAliveConditionSystem), typeof(ObsessedHuggingSystem))]
public sealed partial class ObsessedPersistentTargetComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid EntityUid = EntityUid.Invalid;
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string EntityName = "";
}
