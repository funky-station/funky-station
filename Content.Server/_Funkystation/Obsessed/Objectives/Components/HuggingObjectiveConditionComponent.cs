using Content.Server.Objectives.Systems;
using Content.Server.GameTicking.Rules;

namespace Content.Server.Objectives.Components;

[RegisterComponent, Access(typeof(ObsessedHuggingSystem), typeof(ObsessedRuleSystem))]
public sealed partial class HuggingObjectiveConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Hugged = 0f;
};
