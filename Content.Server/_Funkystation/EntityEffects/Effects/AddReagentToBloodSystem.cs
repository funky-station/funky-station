// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.EntityEffects;

namespace Content.Server._Funkystation.EntityEffects.Effects;

public sealed partial class AddReagentToBloodSystem : EntityEffectSystem<BloodstreamComponent, AddReagentToBlood>
{
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;

    protected override void Effect(Entity<BloodstreamComponent> entity, ref EntityEffectEvent<AddReagentToBlood> args)
    {
        if (args.Effect.Reagent is null)
            return;

        var solution = new Solution();
        solution.AddReagent(args.Effect.Reagent, args.Effect.Amount * args.Scale);
        _bloodstream.TryAddToChemicals((entity.Owner, entity.Comp), solution);
    }
}
