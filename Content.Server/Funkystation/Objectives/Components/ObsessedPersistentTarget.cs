using Content.Server.Objectives.Systems;

namespace Content.Server.Funkystation.Objectives.Components;

[RegisterComponent, Access(typeof(KeepAliveConditionSystem), typeof(ObsessedHuggingSystem))]
public sealed partial class ObsessedPersistentTargetComponent : Component
{
    public EntityUid EntityUid = EntityUid.Invalid;
    public string EntityName = "";
}
