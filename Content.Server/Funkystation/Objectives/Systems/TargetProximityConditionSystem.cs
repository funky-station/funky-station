using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Events;
using Content.Shared.Obsessed;

namespace Content.Server.Objectives.Systems;

public sealed class TargetProximityConditionSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly ObsessedRuleSystem _obsessedRuleSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TargetProximityConditionComponent, ObjectiveAfterAssignEvent>(OnProximityAssign);
        SubscribeLocalEvent<TargetProximityConditionComponent, ObjectiveGetProgressEvent>(OnProximityCheck);

        SubscribeNetworkEvent<PlayerProximityEvent>(UpdateProgress);
    }

    private void OnProximityAssign(EntityUid uid, TargetProximityConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
    }

    private void OnProximityCheck(EntityUid uid, TargetProximityConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (_number.GetTarget(uid) == 0)
            args.Progress = 1f;

        if (TryComp<ObsessedComponent>(args.Mind.CurrentEntity, out var obsessedComponent))
        {
            args.Progress = MathF.Min(obsessedComponent.TimeSpent / (_number.GetTarget(uid) * 60), 1f);

            // blahblahblahblah
            // todo: put this shit somewhere else
            if (args.Progress >= 1F && !obsessedComponent.CompletedObjectives.ContainsKey("TargetProximityCondition"))
            {
                obsessedComponent.CompletedObjectives.Add("TargetProximityCondition", true);

                if (obsessedComponent.CompletedObjectives.Count >= 3)
                {
                    _obsessedRuleSystem.ObsessedCompletedObjectives(args.MindId);
                }
            }

            return;
        }

        args.Progress = 0f;
    }

    // TODO: server side verification
    private void UpdateProgress(PlayerProximityEvent _, EntitySessionEventArgs args)
    {
        if (TryComp<ObsessedComponent>(args.SenderSession.AttachedEntity, out var obsessedComponent))
        {
            obsessedComponent.TimeSpent += 5;
        }
    }
}
