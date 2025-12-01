// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Goobstation.Common.Changeling;
using Content.Shared.EntityEffects;

namespace Content.Server._Funkystation.EntityEffects.Effects;

public sealed partial class AdjustLingChemicalsSystem : EntityEffectSystem<ChangelingComponent, AdjustLingChemicals>
{
    protected override void Effect(Entity<ChangelingComponent> entity, ref EntityEffectEvent<AdjustLingChemicals> args)
    {
        var chemicalChange = args.Effect.Amount * args.Scale;
        entity.Comp.Chemicals = MathF.Max(0f, entity.Comp.Chemicals + chemicalChange);
        entity.Comp.Chemicals = MathF.Min(entity.Comp.Chemicals, entity.Comp.MaxChemicals);

        Dirty(entity);
    }
}
