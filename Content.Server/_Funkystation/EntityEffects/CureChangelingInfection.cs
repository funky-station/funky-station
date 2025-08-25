// SPDX-FileCopyrightText: 2025 Eris <erisfiregamer1@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Content.Shared.Changeling;

namespace Content.Server.EntityEffects.Effects
{
    /// <summary>
    /// Default metabolism used for medicine reagents.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class CureChangelingInfection : EntityEffect
    {
        protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("reagent-effect-guidebook-cure-changeling",
                ("chance", Probability)
            );
        }

        public override void Effect(EntityEffectBaseArgs args)
        {
            var entityManager = args.EntityManager;
            entityManager.RemoveComponent<ChangelingInfectionComponent>(args.TargetEntity);
        }
    }
}
