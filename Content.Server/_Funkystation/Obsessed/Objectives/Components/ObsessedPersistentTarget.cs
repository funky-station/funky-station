using Content.Server._Funkystation.Obsessed.Objectives.Systems;
using Content.Server.Objectives.Systems;

namespace Content.Server._Funkystation.Obsessed.Objectives.Components;

[RegisterComponent, Access(typeof(KeepAliveConditionSystem), typeof(ObsessedHuggingSystem))]
public sealed partial class ObsessedPersistentTargetComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public EntityUid EntityUid = EntityUid.Invalid;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public string EntityName = "";
}
