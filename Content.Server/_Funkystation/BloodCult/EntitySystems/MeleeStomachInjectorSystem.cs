// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Content.Server.BloodCult.Components;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Handles injecting reagents from melee weapons into targets' stomachs instead of bloodstream.
/// </summary>
public sealed class MeleeStomachInjectorSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly StomachSystem _stomach = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MeleeStomachInjectorComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<MeleeStomachInjectorComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.HitEntities.Count == 0)
            return;

        // Get the solution to inject from the weapon
        if (!_solutionContainer.TryGetSolution(entity.Owner, entity.Comp.Solution, out var weaponSolution))
            return;

        var totalAmount = entity.Comp.TransferAmount * args.HitEntities.Count;
        if (weaponSolution.Value.Comp.Solution.Volume < totalAmount)
            return;

        // Extract solution from weapon
        var removedSolution = _solutionContainer.SplitSolution(weaponSolution.Value, totalAmount);
        var amountPerTarget = removedSolution.Volume / args.HitEntities.Count;

        foreach (var target in args.HitEntities)
        {
            // Verify target still exists (could be deleted/gibbed during melee event)
            if (!Exists(target))
                continue;

            // Find the target's stomach
            if (!TryComp<BodyComponent>(target, out var body))
                continue;

            // Get all stomach organs
            var stomachs = _body.GetBodyOrganEntityComps<StomachComponent>(new Entity<BodyComponent?>(target, body));
            foreach (var (stomachUid, stomach, _) in stomachs)
            {
                // Verify stomach organ still exists
                if (!Exists(stomachUid))
                    continue;

                // Try to inject into the stomach using the proper StomachSystem method
                var splitSolution = removedSolution.SplitSolution(amountPerTarget);
                if (_stomach.TryTransferSolution(stomachUid, splitSolution, stomach))
                {
                    break; // Only inject into the first stomach found
                }
            }
        }
    }
}

