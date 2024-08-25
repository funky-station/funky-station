using Content.Server.Funkystation.Objectives.Components;
using Content.Server.Mind;
using Content.Server.Objectives.Components;
using Content.Shared.Objectives.Components;
using Content.Shared.Obsessed;

namespace Content.Server.Objectives.Systems;

public sealed partial class ObsessedHuggingSystem : EntitySystem
{
    [Dependency] private readonly NumberObjectiveSystem _number = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HuggingObjectiveConditionComponent, ObjectiveAfterAssignEvent>(OnHugAssigned);
        SubscribeLocalEvent<HuggingObjectiveConditionComponent, ObjectiveGetProgressEvent>(OnHugCheck);
    }

    private void OnHugAssigned(EntityUid uid, HuggingObjectiveConditionComponent conditionComponent, ref ObjectiveAfterAssignEvent args)
    {
        if (!TryComp<ObsessedPersistentTargetComponent>(args.Mind.OwnedEntity, out var targetComponent))
            return;

        if (!TryComp<ObsessedComponent>(args.Mind.OwnedEntity, out var obsessedComponent))
            return;

        obsessedComponent.TargetUid = targetComponent.EntityUid;
        obsessedComponent.TargetName = targetComponent.EntityName;

        conditionComponent.Hugged = 0f;
    }

    private void OnHugCheck(EntityUid uid, HuggingObjectiveConditionComponent conditionComponent, ref ObjectiveGetProgressEvent args)
    {
        args.Progress = HugCheck(conditionComponent, _number.GetTarget(uid), args.Mind.CurrentEntity);
    }

    private float HugCheck(HuggingObjectiveConditionComponent comp, int target, EntityUid? targetEntity)
    {
        if (target == 0)
            return 1f;

        if (TryComp<ObsessedComponent>(targetEntity, out var obsessedComponent))
        {
            comp.Hugged = obsessedComponent.HugAmount;

            return MathF.Min(comp.Hugged / target, 1f);
        }

        comp.Hugged = 0f;
        return 0f;
    }
}
