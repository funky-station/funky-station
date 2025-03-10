using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Objectives.Events;
using Content.Shared.Obsessed;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Obsessed;

public sealed class TargetProximityConditionSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examineSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private EntityQueryEnumerator<HumanoidAppearanceComponent> _query;

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        var time = _timing.CurTime;
        var query = EntityQueryEnumerator<ObsessedComponent>();

        while (query.MoveNext(out var entUid, out var comp))
        {
            if (comp.NextUpdateTime > time)
                continue;

            comp.NextUpdateTime = time + comp.UpdateTimeInterval;
            ProximityCheck(entUid, comp);
        }
    }

    private void ProximityCheck(EntityUid? entity, ObsessedComponent? component)
    {
        _query = _entityManager.EntityQueryEnumerator<HumanoidAppearanceComponent>();

        while (_query.MoveNext(out var humanoid, out _))
        {
            var metaData = MetaData(humanoid);

            if (!metaData.EntityName.Equals(component?.TargetName))
                continue;

            if (entity != null && !_examineSystem.InRangeUnOccluded(entity.Value, humanoid, 32F))
                return;

            var ev = new PlayerProximityEvent(entity!.Value, component.UpdateTimeInterval);
            RaiseLocalEvent(entity.Value, ref ev);
        }
    }
}
