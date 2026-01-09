// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// Reduces seizure build for entities with Neuroaversion trait.
/// </summary>
public sealed partial class ReduceSeizureBuild : EntityEffectBase<ReduceSeizureBuild>
{
    /// <summary>
    /// The amount of seizure build to reduce per unit of reagent metabolized.
    /// </summary>
    [DataField]
    public float ReductionAmount = 0.1f;

    public override string? EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reduce-seizure-build", ("amount", ReductionAmount), ("chance", Probability));
}
