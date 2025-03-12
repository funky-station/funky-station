using Content.Server._Funkystation.Obsessed.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Shared.Mind;
using Content.Shared.Objectives.Components;
using Content.Shared.Objectives.Events;
using Content.Shared.Obsessed;

namespace Content.Server._Funkystation.Obsessed.Objectives.Systems;

public sealed class TargetProximityConditionSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly ObsessedRuleSystem _obsessedRuleSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Components.TargetProximityConditionComponent, ObjectiveAfterAssignEvent>(OnProximityAssign);
        SubscribeLocalEvent<Components.TargetProximityConditionComponent, ObjectiveGetProgressEvent>(OnProximityCheck);

        SubscribeLocalEvent<ObsessedComponent, PlayerProximityEvent>(UpdateProgress);
    }

    private void OnProximityAssign(EntityUid uid, Components.TargetProximityConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
    }

    private void OnProximityCheck(EntityUid uid, Components.TargetProximityConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (_number.GetTarget(uid) == 0)
            args.Progress = 1f;

        if (TryComp<ObsessedComponent>(args.Mind.CurrentEntity, out var obsessedComponent))
        {
            args.Progress = MathF.Min((float) obsessedComponent.TimeSpent.Seconds / (float) (_number.GetTarget(uid) * 60), 1f);


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

    private void UpdateProgress(Entity<ObsessedComponent> comp, ref PlayerProximityEvent args)
    {
        comp.Comp.TimeSpent = comp.Comp.TimeSpent.Add(args.ComponentUpdateTimeInterval);
    }
}
