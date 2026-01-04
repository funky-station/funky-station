// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Handles the vision and movement effects of migraines.
/// </summary>
public sealed class MigraineSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MigraineComponent, ComponentInit>(OnMigraineInit);
        SubscribeLocalEvent<MigraineComponent, ComponentShutdown>(OnMigraineShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MigraineComponent>();
        while (query.MoveNext(out var uid, out var migraine))
        {
            migraine.PulseAccumulator += frameTime;

            // Handle duration countdown if set
            if (migraine.Duration > 0)
            {
                migraine.Duration -= frameTime;
                if (migraine.Duration <= 0)
                {
                    migraine.IsFading = true;
                    migraine.Duration = -1f;

                    _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");
                }
            }

            // If this is a fading component, fade to zero instead of target magnitude
            var targetBlur = migraine.IsFading ? 0f : migraine.BlurryMagnitude;

            if (migraine.IsFading)
            {
                var fadeSpeed = 1.0f / migraine.FadeOutDuration;
                var fadeAmount = fadeSpeed * frameTime;

                migraine.BlurryMagnitude = MathHelper.Lerp(migraine.BlurryMagnitude, 0f, fadeAmount);
                migraine.PulseAmplitude = MathHelper.Lerp(migraine.PulseAmplitude, 0f, fadeAmount);

                targetBlur = migraine.BlurryMagnitude;

                // Remove the component when fade is complete or very close
                if (migraine.BlurryMagnitude <= 0.01f && migraine.PulseAmplitude <= 0.01f)
                {

                    RemComp<MigraineComponent>(uid);
                    continue;
                }
            }

            // Always apply normal interpolation for CurrentBlur (whether fading or not)
            var rampSpeed = targetBlur > migraine.CurrentBlur
                ? migraine.RampUpSpeed
                : (migraine.RampDownSpeed > 0f ? migraine.RampDownSpeed : migraine.RampUpSpeed);

            migraine.CurrentBlur = MathHelper.Lerp(migraine.CurrentBlur, targetBlur, frameTime * rampSpeed);

            Dirty(uid, migraine);
        }
    }

    /// <summary>
    /// Starts the fadeout process for a migraine component.
    /// </summary>
    public void StartFadeOut(EntityUid uid)
    {
        if (!TryComp<MigraineComponent>(uid, out var migraine))
            return;

        migraine.IsFading = true;

        // Remove the slowdown immediately when starting fade
        _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");


        Dirty(uid, migraine);
    }

    private void OnMigraineInit(EntityUid uid, MigraineComponent component, ComponentInit args)
    {
        component.CurrentBlur = MathF.Min(0.01f, component.BlurryMagnitude);
        component.PulseAccumulator = (float) (DateTime.UtcNow.TimeOfDay.TotalSeconds % 1000.0); // awesome seed generation

        // Apply slowdown if we wanna
        if (component.ApplySlowdown)
        {
            // TrySlowdown with given thingy
            _stun.TrySlowdown(uid, TimeSpan.FromHours(1), true, component.SlowdownFactor, component.SlowdownFactor);
        }

        Dirty(uid, component);
    }

    private void OnMigraineShutdown(EntityUid uid, MigraineComponent component, ComponentShutdown args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        if (!component.IsFading)
        {
            component.IsFading = true;

            // Remove the slowdown immediately
            _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");

            // let update handle fade
            return;
        }

        // If we're already fading and this is called again, remove the slowdown
        _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");
    }
}
