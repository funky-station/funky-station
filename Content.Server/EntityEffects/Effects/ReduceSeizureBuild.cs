// SPDX-License-Identifier: MIT

using Content.Server.Traits.Assorted;
using Content.Shared.EntityEffects;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using System;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
/// Reduces seizure build for entities with Neuroaversion trait.
/// </summary>
[UsedImplicitly]
public sealed partial class ReduceSeizureBuild : EntityEffect
{
    /// <summary>
    /// The amount of seizure build to reduce per unit of reagent metabolized.
    /// </summary>
    [DataField("reductionAmount")]
    public float ReductionAmount = 0.1f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reduce-seizure-build", ("amount", ReductionAmount), ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // Calculate the actual reduction based on amount of reagent metabolized
        var actualReduction = ReductionAmount;
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            actualReduction *= reagentArgs.Scale.Float();
        }

        // Clamp that EXPLICITIVE
        actualReduction = MathF.Max(0.001f, MathF.Min(1.0f, actualReduction));

        if (!args.EntityManager.TryGetComponent<NeuroAversionComponent>(args.TargetEntity, out var comp))
            return;

        var amountToSubtract = MathF.Min(actualReduction, comp.SeizureBuild);

        args.EntityManager.EntitySysManager.GetEntitySystem<NeuroAversionSystem>()
            .ModifySeizureBuild(args.TargetEntity, -amountToSubtract);
    }
}
