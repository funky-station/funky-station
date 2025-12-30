// SPDX-FileCopyrightText: 2025 Mora <46364955+TrixxedHeart@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Handles visual effects and movement penalties for migraines.
/// </summary>
public sealed class MigraineSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MigraineComponent, ComponentInit>(OnMigraineInit);
        SubscribeLocalEvent<MigraineComponent, ComponentShutdown>(OnMigraineShutdown);
        SubscribeLocalEvent<MigraineComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MigraineComponent>();
        while (query.MoveNext(out var uid, out var migraine))
        {
            migraine.PulseAccumulator += frameTime;

            // Handle duration countdown
            if (migraine.Duration > 0)
            {
                migraine.Duration -= frameTime;
                if (migraine.Duration <= 0)
                {
                    migraine.IsFading = true;
                    migraine.Duration = -1f;
                    _movementSpeed.RefreshMovementSpeedModifiers(uid);
                }
            }

            // Calculate target blur value
            var targetBlur = migraine.IsFading ? 0f : migraine.BlurryMagnitude;

            // Handle fade-out effects
            if (migraine.IsFading)
            {
                var fadeSpeed = 1.0f / migraine.FadeOutDuration;
                var fadeAmount = fadeSpeed * frameTime;

                migraine.BlurryMagnitude = MathHelper.Lerp(migraine.BlurryMagnitude, 0f, fadeAmount);
                migraine.PulseAmplitude = MathHelper.Lerp(migraine.PulseAmplitude, 0f, fadeAmount);
                targetBlur = migraine.BlurryMagnitude;

                // Remove component when fade is complete
                if (migraine.BlurryMagnitude <= 0.01f && migraine.PulseAmplitude <= 0.01f)
                {
                    RemComp<MigraineComponent>(uid);
                    continue;
                }
            }

            // Apply blur interpolation
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

        _movementSpeed.RefreshMovementSpeedModifiers(uid);

        Dirty(uid, migraine);
    }

    private void OnRefreshMovementSpeed(EntityUid uid, MigraineComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        // Only apply slowdown if component is active and not fading
        if (!component.IsFading && component.ApplySlowdown)
        {
            args.ModifySpeed(component.SlowdownFactor, component.SlowdownFactor);
        }
    }

    private void OnMigraineInit(EntityUid uid, MigraineComponent component, ComponentInit args)
    {
        // Initialize visual state
        component.CurrentBlur = MathF.Min(0.01f, component.BlurryMagnitude);
        component.PulseAccumulator = (float)(DateTime.UtcNow.TimeOfDay.TotalSeconds % 1000.0);

        // Refresh movement speed modifiers to apply slowdown if enabled
        if (component.ApplySlowdown)
        {
            _movementSpeed.RefreshMovementSpeedModifiers(uid);
        }

        Dirty(uid, component);
    }

    private void OnMigraineShutdown(EntityUid uid, MigraineComponent component, ComponentShutdown args)
    {
        if (TerminatingOrDeleted(uid))
            return;

        _movementSpeed.RefreshMovementSpeedModifiers(uid);

        // Start fade if not already fading
        if (!component.IsFading)
        {
            component.IsFading = true;
        }
    }
}
