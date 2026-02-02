using Content.Server._Funkystation.Genetics.Mutations.Components;
using Content.Shared.Actions;
using Content.Shared.FixedPoint;
using Content.Shared._Funkystation.Genetics.Events;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server._Funkystation.Genetics.Mutations.Systems;

public sealed class MutationAdrenalineRushSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    private const string ReagentId = "Epinephrine";
    private const float Amount = 10f;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MutationAdrenalineRushComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<MutationAdrenalineRushComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MutationAdrenalineRushComponent, AdrenalineRushActionEvent>(OnActionPerformed);
    }

    private void OnInit(Entity<MutationAdrenalineRushComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.GrantedAction, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<MutationAdrenalineRushComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.GrantedAction is { Valid: true } action)
            _actions.RemoveAction(action);
    }

    private void OnActionPerformed(Entity<MutationAdrenalineRushComponent> ent, ref AdrenalineRushActionEvent args)
    {
        if (args.Handled || args.Performer != ent.Owner)
            return;

        args.Handled = true;

        if (!_solution.TryGetInjectableSolution(ent.Owner, out var solution, out _))
            return;

        var quantity = FixedPoint2.New(Amount);
        _solution.TryAddReagent(solution.Value, ReagentId, quantity);
    }
}
