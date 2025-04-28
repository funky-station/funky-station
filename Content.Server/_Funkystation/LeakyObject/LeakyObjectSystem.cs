
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

public sealed class LeakyObjectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();
        _sawmill = _logManager.GetSawmill("leaking");
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityManager.EntityQueryEnumerator<LeakyObjectComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;
            comp.NextUpdate = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateTime);

            // unlike the opinion of whoever made the smoking system, this is fine actually
            if (!_solutionContainers.TryGetSolution(uid, comp.SolutionName, out var soln, out var solution) ||
                !_containers.TryGetContainingContainer((uid, null, null), out var container) ||
                !TryComp(container.Owner, out BloodstreamComponent? bloodstream))
            {
                continue;
            }

            var leakedSolution = _solutionContainers.SplitSolution(soln.Value, comp.TransferAmount);
            leakedSolution.ScaleSolution(comp.LeakEfficiency);

            _reactiveSystem.DoEntityReaction(container.Owner, leakedSolution, ReactionMethod.Touch);
            _bloodstreamSystem.TryAddToChemicals(container.Owner, leakedSolution, bloodstream);
        }
    }
}
