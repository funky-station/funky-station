using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Traits.Assorted;
using Robust.Shared.Timing;

namespace Content.Shared.Drunk;

public abstract class SharedDrunkSystem : EntitySystem
{
    [ValidatePrototypeId<StatusEffectPrototype>]
    public const string DrunkKey = "Drunk";
    public const float maxDrunkTime = 1500; // Funky - caps drunk time to prevent permanent drunkeness. In seconds. 

    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedSlurredSystem _slurredSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!; // Funky - needed to calculate remaining status time. 

    public void TryApplyDrunkenness(EntityUid uid, float boozePower, bool applySlur = true,
        StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        if (TryComp<LightweightDrunkComponent>(uid, out var trait))
            boozePower *= trait.BoozeStrengthMultiplier;

        if (applySlur)
        {
            _slurredSystem.DoSlur(uid, TimeSpan.FromSeconds(boozePower), status);
        }

        if (!_statusEffectsSystem.HasStatusEffect(uid, DrunkKey, status))
        {
            _statusEffectsSystem.TryAddStatusEffect<DrunkComponent>(uid, DrunkKey, TimeSpan.FromSeconds(boozePower), true, status);
        }
        // Funky modification starts here
        else if (_statusEffectsSystem.TryGetTime(uid, DrunkKey, out var time))
        {
            var timeLeft = (float) (time.Value.Item2 - _timing.CurTime).TotalSeconds;

            if (timeLeft + boozePower > maxDrunkTime) 
                _statusEffectsSystem.TrySetTime(uid, DrunkKey, TimeSpan.FromSeconds(maxDrunkTime), status);
            else 
                _statusEffectsSystem.TryAddTime(uid, DrunkKey, TimeSpan.FromSeconds(boozePower), status);
        }
        // Funky modification ends
        // else
        // {
        //     _statusEffectsSystem.TryAddTime(uid, DrunkKey, TimeSpan.FromSeconds(boozePower), status);
        // }
    }

    public void TryRemoveDrunkenness(EntityUid uid)
    {
        _statusEffectsSystem.TryRemoveStatusEffect(uid, DrunkKey);
    }
    public void TryRemoveDrunkenessTime(EntityUid uid, double timeRemoved)
    {
        _statusEffectsSystem.TryRemoveTime(uid, DrunkKey, TimeSpan.FromSeconds(timeRemoved));
    }

}
