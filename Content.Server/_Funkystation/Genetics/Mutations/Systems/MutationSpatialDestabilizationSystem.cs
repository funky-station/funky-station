using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Teleportation;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationSpatialDestabilizationSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly TeleportSystem _teleport = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationSpatialDestabilizationComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, MutationSpatialDestabilizationComponent comp, ComponentInit args)
    {
        ScheduleNextTeleport(comp);
    }

    private void ScheduleNextTeleport(MutationSpatialDestabilizationComponent comp)
    {
        var delay = _random.NextFloat(comp.MinInterval, comp.MaxInterval);
        comp.NextTeleportTime = _timing.CurTime + TimeSpan.FromSeconds(delay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutationSpatialDestabilizationComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextTeleportTime)
                continue;

            _teleport.RandomTeleport(uid, comp.TeleportRadius, comp.Sound, comp.TeleportAttempts);

            ScheduleNextTeleport(comp);
        }
    }
}
