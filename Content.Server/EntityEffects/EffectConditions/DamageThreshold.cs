// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.EntityEffects;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.EffectConditions;

/// <summary>
/// Condition that checks if a specific damage group or type is below a threshold.
/// Used for effects like Ambuzol IV that behave differently based on burn damage.
/// </summary>
public sealed partial class DamageThreshold : EntityEffectCondition
{
    /// <summary>
    /// Damage group or type to check (e.g., "Burn", "Brute", "Caustic")
    /// </summary>
    [DataField(required: true)]
    public string Damage = default!;

    /// <summary>
    /// Maximum damage value. Condition passes if damage is less than or equal to this.
    /// </summary>
    [DataField]
    public FixedPoint2 Max = FixedPoint2.MaxValue;

    /// <summary>
    /// Minimum damage value. Condition passes if damage is greater than or equal to this.
    /// </summary>
    [DataField]
    public FixedPoint2 Min = FixedPoint2.Zero;

    public override bool Condition(EntityEffectBaseArgs args)
    {
        if (!args.EntityManager.TryGetComponent(args.TargetEntity, out DamageableComponent? damage))
            return false;

        FixedPoint2 damageValue = FixedPoint2.Zero;

        // Check if it's a damage group first
        if (damage.DamagePerGroup.TryGetValue(Damage, out var groupDamage))
        {
            damageValue = groupDamage;
        }
        // Otherwise check if it's a damage type
        else if (damage.Damage.DamageDict.TryGetValue(Damage, out var typeDamage))
        {
            damageValue = typeDamage;
        }
        else
        {
            return false;
        }

        return damageValue >= Min && damageValue <= Max;
    }

    public override string GuidebookExplanation(IPrototypeManager prototype)
    {
        return Loc.GetString("reagent-effect-condition-guidebook-damage-threshold",
            ("damage", Damage),
            ("max", Max == FixedPoint2.MaxValue ? (float) int.MaxValue : Max.Float()),
            ("min", Min.Float()));
    }
}
