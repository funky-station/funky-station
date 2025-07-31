// SPDX-FileCopyrightText: 2025 Eris <erisfiregamer1@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    /// <summary>
    /// Default metabolism used for medicine reagents.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class CureHearingLoss : EntityEffect
    {
        /// <summary>
        /// Amount of hearing damage to modify.
        /// </summary>
        [DataField]
        public int Amount = -5;

        protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("reagent-effect-guidebook-cure-changeling",
                ("chance", Probability)
            );
        }

        public override void Effect(EntityEffectBaseArgs args)
        {
            if (args is not EntityEffectReagentArgs)
                return;
            var entityManager = args.EntityManager;
            if (!entityManager.TryGetComponent<SensitiveHearingComponent>(args.TargetEntity, out var sensitiveHearing))
                return;
            entityManager.EntitySysManager.GetEntitySystem<SensitiveHearingSystem>().HealHearingLoss(sensitiveHearing, args.TargetEntity);
        }
    }
}
