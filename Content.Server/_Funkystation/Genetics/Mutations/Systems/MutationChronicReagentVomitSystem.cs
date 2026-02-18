using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Server.Fluids.EntitySystems;
using Content.Server.Forensics;
using Content.Server.Medical;
using Content.Shared.Chemistry.Components;
using Content.Shared.FixedPoint;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationChronicReagentVomitSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly VomitSystem _vomit = default!;
    [Dependency] private readonly PuddleSystem _puddle = default!;
    [Dependency] private readonly ForensicsSystem _forensics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationChronicReagentVomitComponent, ComponentInit>(OnInit);
    }

    private void OnInit(EntityUid uid, MutationChronicReagentVomitComponent comp, ComponentInit args)
    {
        ScheduleNextVomit(uid, comp);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MutationChronicReagentVomitComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextVomitTime)
                continue;

            if (!_random.Prob(comp.Chance))
            {
                ScheduleNextVomit(uid, comp);
                continue;
            }

            PerformVomit(uid, comp);
            ScheduleNextVomit(uid, comp);
        }
    }

    private void PerformVomit(EntityUid uid, MutationChronicReagentVomitComponent comp)
    {
        var amount = FixedPoint2.New(_random.Next(comp.MinAmount, comp.MaxAmount));

        // Create a solution with just our reagent
        var solution = new Solution();
        solution.AddReagent(comp.Reagent, amount);

        // Use the real VomitSystem logic but override the solution
        // First, apply hunger/thirst/slowdown via normal vomit
        _vomit.Vomit(uid, thirstAdded: -30f, hungerAdded: -30f);

        // Then, add solution to puddle
        if (TryComp<TransformComponent>(uid, out var xform))
        {
            if (_puddle.TrySpillAt(xform.Coordinates, solution, out var puddleUid))
            {
                // Transfer DNA so forensics works
                _forensics.TransferDna(puddleUid, uid, false);
            }
        }
    }

    private void ScheduleNextVomit(EntityUid uid, MutationChronicReagentVomitComponent comp)
    {
        var delay = TimeSpan.FromSeconds(_random.NextFloat(comp.MinInterval, comp.MaxInterval));
        comp.NextVomitTime = _timing.CurTime + delay;
    }
}
