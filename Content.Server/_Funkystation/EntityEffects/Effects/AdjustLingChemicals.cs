// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.EntityEffects.Effects;

public sealed partial class AdjustLingChemicals : EntityEffectBase<AdjustLingChemicals>
{
    [DataField]
    public float Amount;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-adjust-ling-chemicals",
            ("chance", Probability),
            ("amount", Amount),
            ("deltasign", Amount >= 0 ? 1 : -1));
}
