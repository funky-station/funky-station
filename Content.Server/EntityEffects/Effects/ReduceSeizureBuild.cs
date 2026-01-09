// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Traits.Assorted;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using System;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
/// Reduces seizure build for entities with Neuroaversion trait.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T,TEffect}"/>
public sealed partial class ReduceSeizureBuildEntityEffectSystem : EntityEffectSystem<NeuroAversionComponent, ReduceSeizureBuild>
{
    [Dependency] private readonly NeuroAversionSystem _neuroAversion = default!;

    protected override void Effect(Entity<NeuroAversionComponent> entity, ref EntityEffectEvent<ReduceSeizureBuild> args)
    {
        // Calculate the actual reduction based on amount of reagent metabolized
        var actualReduction = args.Effect.ReductionAmount * args.Scale;

        // Clamp that EXPLICITIVE
        actualReduction = MathF.Max(0.001f, MathF.Min(1.0f, actualReduction));

        var amountToSubtract = MathF.Min(actualReduction, entity.Comp.SeizureBuild);

        _neuroAversion.ModifySeizureBuild(entity, -amountToSubtract);
    }
}
