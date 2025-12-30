// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;

namespace Content.Shared.EntityConditions.Conditions.Body;

public sealed partial class BloodReagentConditionSystem : EntityConditionSystem<BloodstreamComponent, BloodReagentCondition>
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionSystem = default!;

    protected override void Condition(Entity<BloodstreamComponent> entity, ref EntityConditionEvent<BloodReagentCondition> args)
    {
        // If no reagent specified, condition passes
        if (args.Condition.Reagent is null)
        {
            args.Result = true;
            return;
        }

        // Try to resolve the chemical solution
        if (!_solutionSystem.ResolveSolution(entity.Owner, entity.Comp.BloodSolutionName, ref entity.Comp.BloodSolution, out var chemSolution))
        {
            args.Result = true;
            return;
        }

        // Check if the reagent exists in the solution
        var reagentID = new ReagentId(args.Condition.Reagent, null);
        if (!chemSolution.TryGetReagentQuantity(reagentID, out var quant))
        {
            args.Result = false;
            return;
        }

        // Check if the quantity is within the threshold range
        args.Result = quant >= args.Condition.Min && quant <= args.Condition.Max;
    }
}
