// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;

namespace Content.Shared.EntityConditions.Conditions.Body;

public sealed partial class AddReagentToBloodSystem : EntityEffectSystem<BloodstreamComponent, AddReagentToBlood>
{
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    protected override void Effect(Entity<BloodstreamComponent> entity, ref EntityEffectEvent<AddReagentToBlood> args)
    {
        // If no reagent specified, do nothing
        if (args.Effect.Reagent is null)
            return;

        // Create a solution with the specified reagent and amount
        var solution = new Solution();
        solution.AddReagent(args.Effect.Reagent, args.Effect.Amount * args.Scale);

        // Add the solution to the entity's bloodstream chemicals
        _bloodstream.TryAddToChemicals((entity.Owner, entity.Comp), solution);
    }
}
