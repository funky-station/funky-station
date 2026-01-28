// SPDX-FileCopyrightText: 2026 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Server.Jobs;
using Content.Server.Mind;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.StatusEffect;
using Content.Shared.Traits.Assorted;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// Server-side system for Neuroaversion trait.
/// Handles migraines, seizures, and mindshield interactions.
/// </summary>
public sealed class NeuroAversionSystem : SharedNeuroAversionSystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SeizureSystem _seizure = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private EntityQuery<MindShieldComponent> _mindShieldQuery;
    private EntityQuery<ChronicMigrainesComponent> _chronicMigrainesQuery;

    private const string StatusEffectKey = "Migraine";

    public override void Initialize()
    {
        base.Initialize();

        _mindShieldQuery = GetEntityQuery<MindShieldComponent>();
        _chronicMigrainesQuery = GetEntityQuery<ChronicMigrainesComponent>();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var deltaTime = TimeSpan.FromSeconds(frameTime);

        var query = EntityQueryEnumerator<NeuroAversionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            // Dead entities don't have migraines or seizures
            if (MobState.IsDead(uid))
                continue;

            UpdateMindshieldStatus(uid, comp);

            // Only process if mindshielded
            if (!comp.IsMindShielded)
                continue;

            var missingHpFrac = CalculateHealthFraction(uid);

            UpdateMigraineTimer(uid, comp, frameTime, missingHpFrac);
            UpdateSeizureLogic(uid, comp, deltaTime, missingHpFrac);
        }
    }

    private void UpdateMindshieldStatus(EntityUid uid, NeuroAversionComponent comp)
    {
        var hasMindShield = _mindShieldQuery.HasComponent(uid);
        comp.IsMindShielded = hasMindShield;
        // Check if started mindshielded only once
        if (!comp.StartedMindShieldedChecked)
        {
            comp.StartedMindShielded = comp.IsMindShielded && JobHasMindshieldComponent(uid);
            comp.StartedMindShieldedChecked = true;
        }
    }

    private float CalculateHealthFraction(EntityUid uid)
    {
        if (!TryComp<DamageableComponent>(uid, out var damageable))
            return 0f;

        return CalculateMissingHpFraction(damageable);
    }

    private void UpdateMigraineTimer(EntityUid uid, NeuroAversionComponent comp, float frameTime, float missingHpFrac)
    {
        // Migraines trigger faster when more damaged
        var migraineFactor = 1f - (0.5f * (1f - missingHpFrac));
        var severityMultiplier = comp.StartedMindShielded
            ? comp.StartedMindShieldedMultiplier
            : comp.MidRoundMindShieldedMultiplier;

        var seconds = frameTime * (1f / migraineFactor) * severityMultiplier;
        comp.NextMigraineTime -= TimeSpan.FromSeconds(seconds);

        if (comp.NextMigraineTime <= TimeSpan.Zero)
        {
            // Pick new migraine time
            var nextMigraineSeconds = Random.NextFloat((float)comp.TimeBetweenMigraines.Min.TotalSeconds, (float)comp.TimeBetweenMigraines.Max.TotalSeconds);
            comp.NextMigraineTime = TimeSpan.FromSeconds(nextMigraineSeconds);
            var durationSeconds = Random.NextFloat((float)comp.MigraineDuration.Min.TotalSeconds, (float)comp.MigraineDuration.Max.TotalSeconds);
            TriggerMigraine(uid, durationSeconds);
        }
    }

    private void UpdateSeizureLogic(EntityUid uid, NeuroAversionComponent comp, TimeSpan deltaTime, float missingHpFrac)
    {
        var seconds = (float) deltaTime.TotalSeconds;
        if (seconds <= 0f)
            return;

        var isCritical = MobState.IsCritical(uid);
        var conditionMult = GetConditionMultiplier(comp, isCritical, missingHpFrac);

        // Check for chronic migraines trait interaction
        var hasChronicMigraines = _chronicMigrainesQuery.HasComponent(uid);
        var traitInteractionMult = hasChronicMigraines
            ? ChronicMigraineInteractionMultiplier
            : 1.3f;

        if (!comp.SeizurePaused)
        {
            UpdateSeizureBuild(uid, comp, deltaTime, conditionMult, traitInteractionMult, missingHpFrac);
        }

        // No seizures while critical, already seizing, or paused
        if (isCritical || _seizure.IsSeizing(uid) || comp.SeizurePaused)
            return;

        // Countdown to next hazard check
        comp.NextSeizureCheckTime -= deltaTime;
        if (comp.NextSeizureCheckTime > TimeSpan.Zero)
            return;

        comp.NextSeizureCheckTime = comp.SeizureCheckInterval;

        // Mindshield severity
        float mindshieldMult = comp.StartedMindShielded
            ? comp.StartedMindShieldedMultiplier
            : comp.MidRoundMindShieldedMultiplier;

        // Calculate hazard (per second)
        float hazard =
            (comp.SeizureBuild * comp.BuildHazardFactor);

        hazard *= conditionMult * mindshieldMult * traitInteractionMult;

        float interval = (float)comp.SeizureCheckInterval.TotalSeconds;
        float probability = 1f - MathF.Exp(-hazard * interval);

        double roll = Random.NextDouble();
        bool triggered = roll < probability;
        if (triggered)
        {
            // Use the effects system to trigger a seizure
            var effect = new SeizureSystem.TriggerSeizureEffect { SeizureDuration = (float)comp.NeuroAversionSeizureDuration.TotalSeconds };
            effect.Effect(new EntityEffectBaseArgs(uid, EntityManager));
        }
    }

    private void TriggerMigraine(EntityUid uid, float duration)
    {
        // Extend duration if chronic migraines present
        if (_chronicMigrainesQuery.HasComponent(uid))
        {
            duration += ChronicMigraineDurationBonus;
        }

        _statusEffects.TryAddStatusEffect<MigraineComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(duration), false);

        try
        {
            _popup.PopupEntity(Loc.GetString("trait-neuro-aversion-migraine-start"), uid, uid, PopupType.MediumCaution);
            var othersMessage = Loc.GetString("trait-neuro-aversion-migraine-start-other", ("target", Identity.Entity(uid, EntityManager)));
            _popup.PopupPredicted(Loc.GetString("trait-neuro-aversion-migraine-start"), othersMessage, uid, uid, PopupType.MediumCaution);
        }
        catch
        {
            // Silently fail if localization missing
        }
    }

    /// <summary>
    /// Checks if the entity's job includes mindshield component or implant.
    /// </summary>
    private bool JobHasMindshieldComponent(EntityUid uid)
    {
        if (!_mindSystem.TryGetMind(uid, out _, out var mindComponent))
            return false;

        foreach (var roleUid in mindComponent.MindRoles)
        {
            if (!EntityManager.EntityExists(roleUid) ||
                !EntityManager.TryGetComponent(roleUid, out MindRoleComponent? role) ||
                role.JobPrototype is not { } jobProtoId ||
                !_prototypeManager.TryIndex(jobProtoId, out JobPrototype? jobProto))
                continue;

            foreach (var special in jobProto.Special)
            {
                // We check for both AddComponentSpecial and AddImplantSpecial because while they're ONLY supposed to be given via AddImplantSpecial I don't want the scenario where for some fucking reason someone decides to use AddComponentSpecial to do it instead.
                if ((special is AddComponentSpecial addCompSpecial && addCompSpecial.Components.ContainsKey("MindShield")) ||
                    (special is AddImplantSpecial implantSpecial && implantSpecial.Implants.Any(id => id.Equals("MindShieldImplant", StringComparison.OrdinalIgnoreCase))))
                    return true;
            }
        }

        return false;
    }
}
