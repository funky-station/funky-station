// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Changeling;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.EntityEffects.EffectConditions;

public sealed partial class LingCondition : EntityEffectCondition
{
    public override bool Condition(EntityEffectBaseArgs args)
    {
        return args.EntityManager.HasComponent<ChangelingComponent>(args.TargetEntity);
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-ling");
    }
}