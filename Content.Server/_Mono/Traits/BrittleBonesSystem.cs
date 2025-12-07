// SPDX-FileCopyrightText: 2025 SaffronFennec <firefoxwolf2020@protonmail.com>
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
        // When the component is added, modify the critical threshold
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            // Get current critical threshold
            if (!_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var critThreshold))
                return;

            // Set new critical threshold with the modifier
            _mobThresholdSystem.SetMobStateThreshold(ent, critThreshold.Value + ent.Comp.CriticalThresholdModifier, MobState.Critical);
        }
    }

    private void OnRemove(Entity<BrittleBonesComponent> ent, ref ComponentRemove args)
    {
        // When the component is removed, restore the original critical threshold
        if (TryComp<MobThresholdsComponent>(ent, out var thresholds))
        {
            // Get current critical threshold
            if (!_mobThresholdSystem.TryGetThresholdForState(ent, MobState.Critical, out var critThreshold))
                return;

            // Restore original critical threshold by removing the modifier
            _mobThresholdSystem.SetMobStateThreshold(ent, critThreshold.Value - ent.Comp.CriticalThresholdModifier, MobState.Critical);
        }
    }
}
