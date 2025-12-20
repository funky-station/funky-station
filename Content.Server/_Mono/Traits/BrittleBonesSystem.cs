// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Traits.BrittleBones;

namespace Content.Server.Traits.BrittleBones;

/// <summary>
/// This handles modifying the critical health threshold for entities with brittle bones
/// </summary>
public sealed class BrittleBonesSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe to when a mob with brittle bones gets initialized
        SubscribeLocalEvent<BrittleBonesComponent, ComponentInit>(OnInit);

        // Subscribe to when a mob with brittle bones gets removed
        SubscribeLocalEvent<BrittleBonesComponent, ComponentRemove>(OnRemove);
    }

    private void OnInit(Entity<BrittleBonesComponent> ent, ref ComponentInit args)
    {
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            ApplyThresholdModifier(ent, ent.Comp.CriticalThresholdModifier, thresholds);
        }
    }

    private void OnRemove(Entity<BrittleBonesComponent> ent, ref ComponentRemove args)
    {
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            // Restore original thresholds by removing the modifier (adding the inverse)
            ApplyThresholdModifier(ent, -ent.Comp.CriticalThresholdModifier, thresholds);
        }
    }

    private void ApplyThresholdModifier(EntityUid uid, FixedPoint2 modifier, MobThresholdsComponent thresholds)
    {
        // Shifts softcrit, hardcrit, standard crit, and dead thresholds
        var statesToCheck = new[]
        {
            MobState.SoftCritical,
            MobState.Critical,
            MobState.HardCritical,
            MobState.Dead
        };

        foreach (var state in statesToCheck)
        {
            if (_mobThresholdSystem.TryGetThresholdForState(uid, state, out var currentThreshold, thresholds))
            {
                _mobThresholdSystem.SetMobStateThreshold(uid, currentThreshold.Value + modifier, state, thresholds);
            }
        }
    }
}
