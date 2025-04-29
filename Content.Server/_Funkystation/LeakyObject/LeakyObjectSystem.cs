
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Electrocution;
using Content.Shared.Inventory;
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
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
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

            // i don't actually like that this makes insuls immune, i would prefer it if the janitorial gloves were immune instead, but they don't have a special component
            // TODO: give jani gloves a component like that and use it here instead.
            if (_inventorySystem.TryGetSlotEntity(container.Owner, "gloves", out var glovesUid) &&
                EntityManager.HasComponent<InsulatedComponent>(glovesUid) &&
                !(_inventorySystem.TryGetSlotEntity(container.Owner, "head", out var headUid) && headUid == uid))
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
