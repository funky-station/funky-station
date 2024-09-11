using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(ObsessedRuleSystem))]
public sealed partial class ObsessedRuleComponent : Component
{
    public readonly List<ProtoId<EntityPrototype>> Objectives =
    [
        "ObsessedKeepAliveObjective",
        "ObsessedHugObjective",
        "ObsessedProximityObjective",
        "ObsessedKillRandomPersonObjective",
    ];

    public readonly EntProtoId Murder = "ObsessedKillKeepAliveObjective";

}
