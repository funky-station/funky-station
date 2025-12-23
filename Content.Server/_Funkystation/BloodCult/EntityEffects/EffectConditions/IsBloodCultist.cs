// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Shared.BloodCult;
using Content.Shared.EntityConditions;
using JetBrains.Annotations;
using Robust.Shared.IoC;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntityEffects.EffectConditions;

/// <summary>
/// Condition that checks if an entity is a Blood Cultist.
/// Used for effects that should only affect cultists (or non-cultists if inverted).
/// </summary>
[UsedImplicitly]
public sealed partial class IsBloodCultist : EntityCondition
{
    [DataField]
    public bool Invert = false;

    public override bool RaiseEvent(EntityUid target, IEntityConditionRaiser raiser)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();

        if (!entMan.EntityExists(target))
            return false;

        var isCultist = entMan.HasComponent<BloodCultistComponent>(target);
        return Invert ? !isCultist : isCultist;
    }

    public override string EntityConditionGuidebookText(IPrototypeManager prototype)
    {
        return Loc.GetString(
            "reagent-effect-condition-guidebook-is-blood-cultist",
            ("invert", Invert));
    }
}

