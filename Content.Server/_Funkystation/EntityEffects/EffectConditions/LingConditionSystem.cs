// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Goobstation.Common.Changeling;
using Content.Shared.Changeling;
using Content.Shared.EntityConditions;

namespace Content.Server._Funkystation.EntityEffects.EffectConditions;

public sealed class LingConditionSystem : EntityConditionSystem<ChangelingComponent, LingCondition>
{
    protected override void Condition(Entity<ChangelingComponent> entity, ref EntityConditionEvent<LingCondition> args)
    {
        args.Result = true; // Entity has ChangelingComponent, so condition is met
    }
}
