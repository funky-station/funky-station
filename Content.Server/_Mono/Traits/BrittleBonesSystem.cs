// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Content.Shared.Traits.BrittleBones;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

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
        // When the component is added, modify the critical thresholds
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            var mod = ent.Comp.CriticalThresholdModifier;

            // Modify SoftCritical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.SoftCritical, out var softCritThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, softCritThreshold.Value + mod, MobState.SoftCritical);
            }

            // Modify Critical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var critThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, critThreshold.Value + mod, MobState.Critical);
            }

            // Modify HardCritical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.HardCritical, out var hardCritThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, hardCritThreshold.Value + mod, MobState.HardCritical);
            }
        }
    }

    private void OnRemove(Entity<BrittleBonesComponent> ent, ref ComponentRemove args)
    {
        // When the component is removed, restore the original critical thresholds
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            var mod = ent.Comp.CriticalThresholdModifier;

            // Restore SoftCritical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.SoftCritical, out var softCritThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, softCritThreshold.Value - mod, MobState.SoftCritical);
            }

            // Restore Critical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var critThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, critThreshold.Value - mod, MobState.Critical);
            }

            // Restore HardCritical
            if (_mobThresholdSystem.TryGetThresholdForState(ent, MobState.HardCritical, out var hardCritThreshold))
            {
                _mobThresholdSystem.SetMobStateThreshold(ent, hardCritThreshold.Value - mod, MobState.HardCritical);
            }
        }
    }
}
