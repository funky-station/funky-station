// SPDX-License-Identifier: MIT

using Content.Shared.Mindshield.Components;
using Content.Shared.StatusEffect;
using Content.Shared.Traits.Assorted;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Server.Mind;
using Content.Server.Jobs;
using Content.Shared.Roles.Jobs;

namespace Content.Server.Traits.Assorted;

/// <summary>
/// Handles Neuroaversion trait. When someone has this trait AND has a mindshield they get migraines (from MigraineSystem) and rare seizures (handled in this system).
/// </summary>
public sealed class NeuroAversionSystem : EntitySystem
{
    [Dependency] private readonly Robust.Shared.Random.IRobustRandom _random = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly SeizureSystem _seizure = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedJobSystem _jobSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    private EntityQuery<MindShieldComponent> _mindShieldQuery;
    private EntityQuery<ChronicMigrainesComponent> _chronicMigrainesQuery;

    private const string StatusEffectKey = "Migraine"; // used for generic migraines

    // get the passive-build multiplier for the current health tier
    private static float GetConditionMultiplier(NeuroAversionComponent comp, bool isCritical, float missingHpFrac)
    {
        float baseMultiplier;

        if (isCritical)
            baseMultiplier = comp.ConditionCriticalMultiplier;
        else if (missingHpFrac >= 2f / 3f)
            baseMultiplier = comp.ConditionBadMultiplier;
        else if (missingHpFrac >= 1f / 3f)
            baseMultiplier = comp.ConditionOkayMultiplier;
        else
            baseMultiplier = comp.ConditionGoodMultiplier;

        // If the entity is not roundstart mindshielded, they get more severe effects.
        var severityMultiplier = comp.StartedMindShielded
            ? comp.StartedMindShieldedMultiplier
            : comp.MidRoundMindShieldedMultiplier;

        return baseMultiplier * severityMultiplier;
    }

    /// <summary>
    /// Checks if the entity's job prototype includes MindShieldComponent or MindShield implants
    /// </summary>
    private bool JobHasMindshieldComponent(EntityUid uid)
    {
        if (!_mindSystem.TryGetMind(uid, out var mindId, out _))
            return false;

        if (!_jobSystem.MindTryGetJob(mindId, out var jobProto))
            return false;

        // Check for AddComponentSpecial with MindShield component
        foreach (var special in jobProto.Special)
        {
            if (special is AddComponentSpecial addCompSpecial)
            {
                if (addCompSpecial.Components.ContainsKey("MindShield"))
                {
                    return true;
                }
            }
        }

        // Check for AddImplantSpecial with mindshield implants
        foreach (var special in jobProto.Special)
        {
            if (special is AddImplantSpecial implantSpecial)
            {
                foreach (var implantId in implantSpecial.Implants)
                {
                    if (implantId == "MindShieldImplant" ||
                        implantId.Contains("MindShield") ||
                        implantId.Contains("mindshield"))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }


    public override void Initialize()
    {
        base.Initialize();

        _mindShieldQuery = GetEntityQuery<MindShieldComponent>();
        _chronicMigrainesQuery = GetEntityQuery<ChronicMigrainesComponent>();

        SubscribeLocalEvent<NeuroAversionComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<NeuroAversionComponent, ComponentShutdown>(OnShutdown);
        // Ideally it would be funny to have a whole system that detects certain keywords a player says negatively about nanotrasen or positively about the syndicate but thats like, severely out of scope of what I want to do and chatcode is scary.
        // This is the only remnant of this system and i only have this here to act as a like "this is what i want done with it" if someone ever wants to implement it properly.
//        SubscribeLocalEvent<NeuroAversionComponent, Content.Server.Chat.Systems.EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnStartup(EntityUid uid, NeuroAversionComponent component, ComponentStartup args)
    {
        // Initialize migraine timer
        component.NextMigraineTime = _random.NextFloat(component.TimeBetweenMigraines.X, component.TimeBetweenMigraines.Y);

        // Check if entity already has mindshield and determine if it's roundstart
        var hasMindShield = _mindShieldQuery.HasComponent(uid);
        component.IsMindShielded = hasMindShield;

        if (hasMindShield)
        {
            component.StartedMindShielded = JobHasMindshieldComponent(uid);
        }
        else
        {
            component.StartedMindShielded = false;
        }
    }

    private void OnShutdown(EntityUid uid, NeuroAversionComponent component, ComponentShutdown args)
    {
        // Clean up migraine effect
        if (!TerminatingOrDeleted(uid))
        {
            _statusEffects.TryRemoveStatusEffect(uid, StatusEffectKey);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<NeuroAversionComponent>();
        while (query.MoveNext(out var uid, out var aversion))
        {
            var hasMindShield = _mindShieldQuery.HasComponent(uid);

            // Update mindshield status and determine if this was a roundstart mindshield
            // Check if their job prototype includes MindShieldComponent
            if (hasMindShield && !aversion.IsMindShielded)
            {
                aversion.IsMindShielded = true;
                aversion.StartedMindShielded = JobHasMindshieldComponent(uid);
            }
            else if (!hasMindShield && aversion.IsMindShielded)
            {
                aversion.IsMindShielded = false;
            }
            else if (hasMindShield && aversion.IsMindShielded && !aversion.StartedMindShielded)
            {
                // Check if this should have been marked as roundstart mindshielded
                aversion.StartedMindShielded = JobHasMindshieldComponent(uid);
            }

            if (!aversion.IsMindShielded)
                continue;

            // Migraine frequency: scale timers based on fraction of missing HP so more injured entities experience migraines more frequently.
            var missingHpFrac = 0f;
            if (TryComp<DamageableComponent>(uid, out var damageable))
            {
                // Use HealthBarThreshold if present as an approximate max otherwise assume 100
                var maxHp = damageable.HealthBarThreshold?.Float() ?? 100f;
                if (maxHp > 0f)
                {
                    var currentDamage = (float)damageable.TotalDamage;
                    missingHpFrac = MathF.Max(0f, MathF.Min(1f, currentDamage / maxHp));
                }
            }

            // Dont allow episodes to trigger if the entity is crit/dead
            // Allow building meter if the entity is in crit however
            var isDead = false;
            var isCritical = false;
            if (TryComp<MobStateComponent>(uid, out var mobState))
            {
                isDead = mobState.CurrentState == MobState.Dead;
                isCritical = mobState.CurrentState == MobState.Critical;
            }

            if (isDead)
                continue;

            // Decrease next migraine timer faster when more hurt, and apply severity multiplier
            var migraineFactor = 1f - (0.5f * (1f - missingHpFrac));

            // Apply severity multiplier for mid-round mindshielded entities
            var severityMultiplier = aversion.StartedMindShielded
                ? aversion.StartedMindShieldedMultiplier
                : aversion.MidRoundMindShieldedMultiplier;

            aversion.NextMigraineTime -= frameTime * (1f / migraineFactor) * severityMultiplier;
            if (aversion.NextMigraineTime <= 0f)
            {
                // Reset next migraine timer. Typical configured timings are 8-20 minutes (480-1200s).
                // TimeBetweenMigraines should be set to something like (480f, 1200f) for that behavior.
                aversion.NextMigraineTime = _random.NextFloat(aversion.TimeBetweenMigraines.X, aversion.TimeBetweenMigraines.Y);
                var duration = _random.NextFloat(aversion.MigraineDuration.X, aversion.MigraineDuration.Y);
                TriggerMigraine(uid, duration);
            }

            // Seizure meter: passive build accumulates over time and is scaled by condition tiers
            // (Good/Okay/Bad/Critical) derived from missing HP fraction.
            // Determine condition multiplier from missingHpFrac
            var conditionMult = GetConditionMultiplier(aversion, isCritical, missingHpFrac);

            // Check for trait interaction with Chronic Migraines
            var hasChronicMigraines = _chronicMigrainesQuery.HasComponent(uid);
            var traitInteractionMultiplier = hasChronicMigraines ? 1.3f : 1.0f; // 30% extra build with both traits

            // Passive build
            var passive = aversion.BaseSeizurePassivePerSec * conditionMult * traitInteractionMultiplier;

            aversion.SeizureBuild += passive * frameTime;

            // Add random jumps to make seizures feel truly random but still health-state dependent
            // More frequent spikes but smaller when healthy, larger when damaged
            var randomSpikeProbability = 0.005f * frameTime * (1f + missingHpFrac * 2f);
            if (_random.NextDouble() < randomSpikeProbability)
            {
                // Smaller spikes in good condition, escalating with damage
                var baseSpike = _random.NextFloat(0.005f, 0.02f); // 0.5-2% base spike
                var damageMultiplier = 1f + (missingHpFrac * 4f); // 1x to 5x multiplier based on damage
                var randomSpike = baseSpike * damageMultiplier;
                aversion.SeizureBuild += randomSpike;
            }

            // CLAMP THAT BITCH
            if (aversion.SeizureBuild < 0f)
                aversion.SeizureBuild = 0f;

            // Handle seizure triggering, rolls every 3 seconds
            // Only attempt to trigger if not dead, not in crit, and not already having a seizure
            if (!isCritical && !_seizure.IsSeizing(uid))
            {
                aversion.NextSeizureRollTime -= frameTime;

                if (aversion.NextSeizureRollTime <= 0f)
                {
                    // Reset timer for next roll
                    aversion.NextSeizureRollTime = 3f;

                    var buildFraction = 0f;
                    if (aversion.SeizureThreshold > 0f)
                        buildFraction = MathF.Max(0f, MathF.Min(1f, aversion.SeizureBuild / aversion.SeizureThreshold));

                    // Calculate seizure chance as a percentage

                    // Chance based on build level with exponential scaling for dramatic effect
                    var scaledBuildFraction = buildFraction * buildFraction * buildFraction;
                    var buildChancePercent = 25f * scaledBuildFraction; // Up to 25% at full build

                    // Health condition multiplier for build-based chance
                    var seizureConditionMult = GetConditionMultiplier(aversion, isCritical, missingHpFrac);
                    var totalChancePercent = buildChancePercent * seizureConditionMult;
                    var chanceDecimal = totalChancePercent / 100f; // Convert to 0-1 range

                    // Roll for seizure
                    var roll = _random.NextDouble();
                    var hit = roll < chanceDecimal;

                    if (hit)
                    {
                        // Trigger seizure and reset build
                        _seizure.StartSeizure(uid, null, 10f);
                        aversion.SeizureBuild = aversion.PostSeizureResidual;
                    }
                }
            }
        }
    }

    private void TriggerMigraine(EntityUid uid, float duration)
    {
        // Check if entity has both Chronic Migraines and Neuro Aversion for trait interaction
        var hasChronicMigraines = _chronicMigrainesQuery.HasComponent(uid);

        // Extend migraine duration if both traits are present
        if (hasChronicMigraines)
        {
            duration += 2f; // Add 2 extra seconds for trait interaction
        }

        // use "Migraine" status effect for migraines because this is bootleg chronic migraines with an extra trigger
        _statusEffects.TryAddStatusEffect<MigraineComponent>(uid, StatusEffectKey, TimeSpan.FromSeconds(duration), false);
        try
        {
            _popup.PopupEntity(Loc.GetString("trait-neuro-aversion-migraine-start"), uid, uid, PopupType.MediumCaution);
            var othersMessage = Loc.GetString("trait-neuro-aversion-migraine-start-other", ("target", Identity.Entity(uid, EntityManager)));
            _popup.PopupPredicted(Loc.GetString("trait-neuro-aversion-migraine-start"), othersMessage, uid, uid, PopupType.MediumCaution);
        }
        catch
        {
            // do nothing!
        }
    }

    /// <summary>
    /// Modifies seizure build on the target entity by <paramref name="amount"/>.
    /// Positive values increase build, negative values decrease build.
    /// </summary>
    public void ModifySeizureBuild(EntityUid uid, float amount)
    {
        if (!TryComp<NeuroAversionComponent>(uid, out var comp))
            return;

        var newBuild = comp.SeizureBuild + amount;
        comp.SeizureBuild = MathF.Max(0f, MathF.Min(comp.SeizureThreshold * 10f, newBuild));
    }


    /// <summary>
    /// Attempts to trigger a seizure on the target entity.
    /// Returns true if the target has a <see cref="NeuroAversionComponent"/> and the action was performed.
    /// </summary>
    public bool TryTriggerSeizure(EntityUid uid)
    {
        if (TerminatingOrDeleted(uid))
            return false;

        if (!TryComp<NeuroAversionComponent>(uid, out var comp))
            return false;

        // Trigger seizure and reset build
        _seizure.StartSeizure(uid);
        comp.SeizureBuild = comp.PostSeizureResidual;
        return true;
    }
}
