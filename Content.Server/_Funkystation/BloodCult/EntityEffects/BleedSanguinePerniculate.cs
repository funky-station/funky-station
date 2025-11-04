// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.BloodCult.EntityEffects;

/// <summary>
/// Makes an entity bleed Sanguine Perniculate instead of their normal blood type, but only if they're already bleeding.
/// Changes what blood they bleed out, not their internal blood.
/// </summary>
public sealed partial class BleedSanguinePerniculate : EntityEffect
{
    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-bleed-sanguine-perniculate", ("chance", Probability));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent<BloodstreamComponent>(args.TargetEntity, out var bloodstream))
            return;

        // Only work if the entity is already bleeding
        if (bloodstream.BleedAmount <= 0)
            return;

        // Store their original blood type if not already stored
        if (!args.EntityManager.TryGetComponent<EdgeEssentiaBloodComponent>(args.TargetEntity, out var edgeEssentiaComp))
        {
            edgeEssentiaComp = args.EntityManager.AddComponent<EdgeEssentiaBloodComponent>(args.TargetEntity);
            edgeEssentiaComp.OriginalBloodReagent = bloodstream.BloodReagent;
        }

        // Change their blood type to Sanguine Perniculate so that when they bleed, it comes out as Sanguine Perniculate
        var bloodstreamSystem = args.EntityManager.System<BloodstreamSystem>();
        bloodstreamSystem.ChangeBloodReagent(args.TargetEntity, "SanguinePerniculate", bloodstream);
    }
}

/// <summary>
/// Component to track the original blood type of an entity affected by Edge Essentia
/// and how much Sanguine Perniculate they've bled for the ritual pool.
/// </summary>
[RegisterComponent]
public sealed partial class EdgeEssentiaBloodComponent : Component
{
    /// <summary>
    /// The original blood reagent before Edge Essentia changed it
    /// </summary>
    [DataField]
    public string OriginalBloodReagent = "Blood";

    /// <summary>
    /// Tracks the last amount of Sanguine Perniculate in the temporary solution to detect new bleeding
    /// </summary>
    [DataField]
    public FixedPoint2 LastTrackedBloodAmount = FixedPoint2.Zero;
}

