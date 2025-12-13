// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Shared._Funkystation.BloodCult.COmponents;
using Content.Shared.BloodCult;
using Content.Shared.EntityConditions;

namespace Content.Shared._Funkystation.BloodCult;

public sealed class IsBloodCultistConditionSystem : EntityConditionSystem<BloodCultistComponent, IsBloodCultistCondition>
{
    protected override void Condition(Entity<BloodCultistComponent> entity, ref EntityConditionEvent<IsBloodCultistCondition> args)
    {
        args.Result = !args.Condition.Invert;
    }
}
