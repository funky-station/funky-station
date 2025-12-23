// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using Content.Shared.BloodCult;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Replaces cultists' blood with Sanguine Perniculate and restores
/// their original blood solution when the cultist component is removed.
/// </summary>
public sealed class BloodCultistMetabolismSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodCultistComponent, ComponentInit>(OnCultistInit);
        SubscribeLocalEvent<BloodCultistComponent, ComponentShutdown>(OnCultistShutdown);
        SubscribeLocalEvent<BloodCultistComponent, EntityTerminatingEvent>(OnCultistTerminating);
    }

    private void OnCultistInit(EntityUid uid, BloodCultistComponent component, ComponentInit args)
    {
        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        if (!_solutions.TryGetSolution(
                (uid, CompOrNull<SolutionContainerManagerComponent>(uid)),
                bloodstream.BloodSolutionName,
                out var solutionEnt))
            return;

        var solution = solutionEnt.Value.Comp.Solution;

        // Store original blood once
        if (component.OriginalBloodSolution == null)
            component.OriginalBloodSolution = solution.Clone();

        var originalVolume = solution.Volume;

        solution.RemoveAllSolution();
        solution.AddReagent("SanguinePerniculate", originalVolume);
    }
    private void OnCultistShutdown(EntityUid uid, BloodCultistComponent component, ComponentShutdown args)
    {
        RestoreBlood(uid, component);
    }

    private void OnCultistTerminating(EntityUid uid, BloodCultistComponent component, ref EntityTerminatingEvent args)
    {
        RestoreBlood(uid, component);
    }

    private void RestoreBlood(EntityUid uid, BloodCultistComponent component)
    {
        if (component.OriginalBloodSolution == null)
            return;

        if (!TryComp<BloodstreamComponent>(uid, out var bloodstream))
            return;

        if (!_solutions.TryGetSolution(
                (uid, CompOrNull<SolutionContainerManagerComponent>(uid)),
                bloodstream.BloodSolutionName,
                out var solutionEnt))
            return;

        var solution = solutionEnt.Value.Comp.Solution;

        solution.RemoveAllSolution();

        foreach (var reagent in component.OriginalBloodSolution.Contents)
        {
            solution.AddReagent(reagent.Reagent, reagent.Quantity);
        }
    }
}
