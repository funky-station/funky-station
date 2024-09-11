using Content.Client.Examine;
using Content.Shared.Humanoid;
using Content.Shared.Objectives.Events;
using Content.Shared.Obsessed;

namespace Content.Client.Obsessed;

public sealed class TargetProximityConditionSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystem _examineSystem = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;

    private bool _initialized;
    private float _updateTime;

    private ObsessedComponent? _obsessedComponent;
    private EntityUid? _entUid;
    private EntityQueryEnumerator<HumanoidAppearanceComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ObsessedComponent, ComponentStartup>(OnComponentStartup);
    }

    public override void Update(float deltaTime)
    {
        _updateTime += deltaTime;

        if (!_initialized)
            return;

        // yea youre tellin me
        if (!(_updateTime > 50f))
            return;

        _updateTime = 0;
        ProximityCheck(_entUid, _obsessedComponent);
    }

    private void OnComponentStartup(EntityUid entity, ObsessedComponent component, ComponentStartup args)
    {
        _initialized = true;
        _obsessedComponent = component;
        _entUid = entity;
    }

    private void ProximityCheck(EntityUid? entity, ObsessedComponent? component)
    {
        if ((entity == null) | (component == null))
            return;

        _query = _entityManager.EntityQueryEnumerator<HumanoidAppearanceComponent>();

        while (_query.MoveNext(out var humanoid, out _))
        {
            TryComp<MetaDataComponent>(humanoid, out var metaData);

            if (metaData == null)
                continue;

            if (!metaData.EntityName.Equals(component?.TargetName))
                continue;

            if (!_examineSystem.InRangeUnOccluded((EntityUid) entity!, metaData.Owner, 32F))
                return;

            RaiseNetworkEvent(new PlayerProximityEvent());
        }
    }
}
