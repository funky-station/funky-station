// SPDX-FileCopyrightText: 2026 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Traits.Assorted;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.EntityEffects;
using Content.Shared.Traits.Assorted;
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

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reduce-seizure-build", ("amount", ReductionAmount), ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        // Calculate the actual reduction based on amount of reagent metabolized
        var actualReduction = ReductionAmount;
        EntityEffectReagentArgs? reagentArgs = args as EntityEffectReagentArgs;
        if (reagentArgs != null)
        {
            actualReduction *= reagentArgs.Scale.Float();
        }

        // Clamp that EXPLICITIVE
        actualReduction = MathF.Max(0.001f, MathF.Min(1.0f, actualReduction));

        if (!args.EntityManager.TryGetComponent<NeuroAversionComponent>(args.TargetEntity, out var comp))
            return;

        // Check if the reagent is still present in the bloodstream
        // this is probably a fucked way to do this
        var solutionSystem = args.EntityManager.System<Content.Shared.Chemistry.EntitySystems.SharedSolutionContainerSystem>();
        Solution? bloodstream = null;
        if (args.EntityManager.TryGetComponent<SolutionContainerManagerComponent>(args.TargetEntity, out var solutions))
        {
            solutionSystem.TryGetSolution(solutions, "bloodstream", out bloodstream, false);
        }
        bool reagentPresent = false;
        if (reagentArgs != null && reagentArgs.Reagent != null && bloodstream != null)
        {
            reagentPresent = bloodstream.ContainsReagent(new Content.Shared.Chemistry.Reagent.ReagentId(reagentArgs.Reagent.ID, null));
        }

        // Pause seizure rolls and build only while reagent is present
        comp.SeizurePaused = reagentPresent;

        var amountToSubtract = MathF.Min(actualReduction, comp.SeizureBuild);
        args.EntityManager.EntitySysManager.GetEntitySystem<NeuroAversionSystem>()
            .ModifySeizureBuild(args.TargetEntity, -amountToSubtract);
    }
}
