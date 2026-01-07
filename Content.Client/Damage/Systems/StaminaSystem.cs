// SPDX-FileCopyrightText: 2025 Drywink <hugogrethen@gmail.com>
// SPDX-FileCopyrightText: 2025 Princess Cheeseballs <66055347+princess-cheeseballs@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Princess Cheeseballs <66055347+pronana@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Princess-Cheeseballs <https://github.com/Princess-Cheeseballs>
//
// SPDX-License-Identifier: MIT

using Content.Client.Stunnable;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Robust.Client.GameObjects;

namespace Content.Client.Damage.Systems;

public sealed partial class StaminaSystem : SharedStaminaSystem
{
    [Dependency] private readonly AnimationPlayerSystem _animation = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly StunSystem _stun = default!; // Clientside Stun System

    private const string StaminaAnimationKey = "stamina";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StaminaComponent, AnimationCompletedEvent>(OnAnimationCompleted);
        SubscribeLocalEvent<ActiveStaminaComponent, ComponentShutdown>(OnActiveStaminaShutdown);
        SubscribeLocalEvent<StaminaComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    protected override void OnStamHandleState(Entity<StaminaComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        base.OnStamHandleState(entity, ref args);

        TryStartAnimation(entity);
    }

    private void OnActiveStaminaShutdown(Entity<ActiveStaminaComponent> entity, ref ComponentShutdown args)
    {
        // If we don't have active stamina, we shouldn't have stamina damage. If the update loop can trust it we can trust it.
        if (!TryComp<StaminaComponent>(entity, out var stamina))
            return;

        StopAnimation((entity, stamina));
    }

    protected override void OnShutdown(Entity<StaminaComponent> entity, ref ComponentShutdown args)
    {
        base.OnShutdown(entity, ref args);

        StopAnimation(entity);
    }

    private void OnMobStateChanged(Entity<StaminaComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            StopAnimation(ent);
    }

    private void TryStartAnimation(Entity<StaminaComponent> entity)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        // If the animation is running, the system should update it accordingly
        // If we're below the threshold to animate, don't try to animate
        // If we're in stamcrit don't override it
        if (entity.Comp.AnimationThreshold > entity.Comp.StaminaDamage || _animation.HasRunningAnimation(entity, StaminaAnimationKey))
            return;

        // Don't animate if we're dead
        if (_mobState.IsDead(entity))
            return;

        entity.Comp.StartOffset = sprite.Offset;

        PlayAnimation((entity, entity.Comp, sprite));
    }

    private void StopAnimation(Entity<StaminaComponent, SpriteComponent?> entity)
    {
        if(!Resolve(entity, ref entity.Comp2))
            return;

        _animation.Stop(entity.Owner, StaminaAnimationKey);
        entity.Comp1.StartOffset = entity.Comp2.Offset;
    }

    private void OnAnimationCompleted(Entity<StaminaComponent> entity, ref AnimationCompletedEvent args)
    {
        if (args.Key != StaminaAnimationKey || !args.Finished || !TryComp<SpriteComponent>(entity, out var sprite))
            return;

        // stop looping if we're below the threshold
        if (entity.Comp.AnimationThreshold > entity.Comp.StaminaDamage)
        {
            _animation.Stop(entity.Owner, StaminaAnimationKey);
            _sprite.SetOffset((entity, sprite), entity.Comp.StartOffset);
            return;
        }

        if (!HasComp<AnimationPlayerComponent>(entity))
            return;

        PlayAnimation((entity, entity.Comp, sprite));
    }

    private void PlayAnimation(Entity<StaminaComponent, SpriteComponent> entity)
    {
        // Validate values to prevent NaN/Infinity propagation
        var staminaDamage = entity.Comp1.StaminaDamage;
        var critThreshold = entity.Comp1.CritThreshold;
        var animationThreshold = entity.Comp1.AnimationThreshold;

        // If any critical values are invalid, skip animation to prevent NaN errors
        if (float.IsNaN(staminaDamage) || float.IsInfinity(staminaDamage) ||
            float.IsNaN(critThreshold) || float.IsInfinity(critThreshold) ||
            float.IsNaN(animationThreshold) || float.IsInfinity(animationThreshold))
        {
            return;
        }

        var denominator = critThreshold - animationThreshold;
        
        // Prevent division by zero - if thresholds are equal, use max step
        float step;
        if (MathF.Abs(denominator) < float.Epsilon)
        {
            step = 1f;
        }
        else
        {
            step = Math.Clamp((staminaDamage - animationThreshold) / denominator,
                0f,
                1f);
        }

        // Validate step isn't NaN before using it in calculations
        if (float.IsNaN(step) || float.IsInfinity(step))
        {
            return;
        }

        var frequency = entity.Comp1.FrequencyMin + step * entity.Comp1.FrequencyMod;
        var jitter = entity.Comp1.JitterAmplitudeMin + step * entity.Comp1.JitterAmplitudeMod;
        var breathing = entity.Comp1.BreathingAmplitudeMin + step * entity.Comp1.BreathingAmplitudeMod;

        // Final validation of calculated values before passing to animation system
        if (float.IsNaN(frequency) || float.IsInfinity(frequency) ||
            float.IsNaN(jitter) || float.IsInfinity(jitter) ||
            float.IsNaN(breathing) || float.IsInfinity(breathing))
        {
            return;
        }

        _animation.Play(entity.Owner,
            _stun.GetFatigueAnimation(entity.Comp2,
                frequency,
                entity.Comp1.Jitters,
                jitter * entity.Comp1.JitterMin,
                jitter * entity.Comp1.JitterMax,
                breathing,
                entity.Comp1.StartOffset,
                ref entity.Comp1.LastJitter),
            StaminaAnimationKey);
    }
}
