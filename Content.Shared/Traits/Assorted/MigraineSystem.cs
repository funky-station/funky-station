// SPDX-License-Identifier: MIT

using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using System;

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

            var rampSpeed = migraine.BlurryMagnitude > migraine.CurrentBlur
                ? migraine.RampUpSpeed
                : (migraine.RampDownSpeed > 0f ? migraine.RampDownSpeed : migraine.RampUpSpeed);
            migraine.CurrentBlur = MathHelper.Lerp(migraine.CurrentBlur, migraine.BlurryMagnitude, frameTime * rampSpeed);

            Dirty(uid, migraine);
        }
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

        // Remove the slowdown
        _statusEffects.TryRemoveStatusEffect(uid, "SlowedDown");
    }
}
