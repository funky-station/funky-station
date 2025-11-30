// SPDX-FileCopyrightText: 2025 Eris <erisfiregamer1@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects
{
    /// <summary>
    /// Cures changeling infection.
    /// </summary>
    [UsedImplicitly]
    public sealed partial class CureChangelingInfection : EntityEffectBase<CureChangelingInfection>
    {
        public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        {
            return Loc.GetString("reagent-effect-guidebook-cure-changeling",
                ("chance", Probability)
            );
        }
    }
}
