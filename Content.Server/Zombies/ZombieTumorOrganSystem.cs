// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-only or MIT

using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Robust.Shared.Collections;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Threading;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.GameObjects;
using System.Linq;

namespace Content.Server.Zombies;

public sealed class ZombieTumorOrganSystem : SharedZombieTumorOrganSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly ZombieSystem _zombieSystem = default!;

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(2);
    private TumorOrganJob _tumorJob;

    public override void Initialize()
    {
        base.Initialize();

        _tumorJob = new TumorOrganJob(_entityLookup, _transform);

        SubscribeLocalEvent<ZombieTumorInfectionComponent, MobStateChangedEvent>(OnInfectionMobStateChanged);
        SubscribeLocalEvent<ZombieTumorInfectionComponent, ComponentRemove>(OnInfectionRemoved);
        SubscribeLocalEvent<ZombieTumorOrganComponent, OrganRemovedFromBodyEvent>(OnTumorOrganRemoved);
        SubscribeLocalEvent<ZombieTumorSpawnerComponent, ComponentInit>(OnTumorSpawnerInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_net.IsClient)
            return;

        var curTime = _timing.CurTime;

        // Update infection progression
        UpdateInfectionProgression(curTime);

        // Update organ infection spread
        UpdateOrganInfectionSpread(curTime);

        // Update tumor spawners (retry failed organ insertions)
        UpdateTumorSpawners(curTime);
    }

    private void UpdateInfectionProgression(TimeSpan curTime)
    {
        var infectionQuery = EntityQueryEnumerator<ZombieTumorInfectionComponent, DamageableComponent, MobStateComponent>();
        while (infectionQuery.MoveNext(out var uid, out var infection, out var damageable, out var mobState))
        {
            // Process damage/effects every second
            if (infection.NextTick > curTime)
                continue;

            infection.NextTick = curTime + TimeSpan.FromSeconds(1);

            // Check if IPC
            if (TryComp<BloodstreamComponent>(uid, out var bloodstream) &&
                bloodstream.BloodReagent == "Oil")
            {
                HandleIPCDamage(uid, infection, damageable, bloodstream);
            }
            else
            {
                HandleNormalDamage(uid, infection, damageable, mobState);
            }

            // Check stage progression
            if (infection.NextStageAt <= curTime)
            {
                ProgressInfectionStage(uid, infection);
            }
        }
    }

    private void HandleIPCDamage(EntityUid uid, ZombieTumorInfectionComponent infection, DamageableComponent damageable, BloodstreamComponent bloodstream)
    {
        // Try to drain oil
        if (_solutionContainer.ResolveSolution(uid, bloodstream.BloodSolutionName, ref bloodstream.BloodSolution, out var bloodSolution))
        {
            var oilAmount = bloodSolution.GetTotalPrototypeQuantity("Oil");
            
            if (oilAmount > 0)
            {
                // Drain oil
                var drainAmount = FixedPoint2.Min(FixedPoint2.New(infection.OilDrainAmount), oilAmount);
                _solutionContainer.RemoveReagent(bloodstream.BloodSolution.Value, new ReagentId("Oil", null), drainAmount);
            }
            else
            {
                // Oil is empty, apply radiation damage
                var multiplier = _mobState.IsCritical(uid) ? infection.CritDamageMultiplier : 1f;
                var damage = infection.RadiationDamage * multiplier;
                _damageable.TryChangeDamage(uid, damage, true, false, damageable);
            }
        }
    }

    private void HandleNormalDamage(EntityUid uid, ZombieTumorInfectionComponent infection, DamageableComponent damageable, MobStateComponent mobState)
    {
        DamageSpecifier damage;
        switch (infection.Stage)
        {
            case ZombieTumorInfectionStage.Early:
                damage = infection.EarlyDamage;
                break;
            case ZombieTumorInfectionStage.TumorFormed:
                damage = infection.TumorDamage;
                break;
            case ZombieTumorInfectionStage.Advanced:
                damage = infection.AdvancedDamage;
                break;
            default:
                return;
        }

        var multiplier = _mobState.IsCritical(uid, mobState) ? infection.CritDamageMultiplier : 1f;
        _damageable.TryChangeDamage(uid, damage * multiplier, true, false, damageable);
    }

    private void ProgressInfectionStage(EntityUid uid, ZombieTumorInfectionComponent infection)
    {
        switch (infection.Stage)
        {
            case ZombieTumorInfectionStage.Early:
                // Form tumor
                infection.Stage = ZombieTumorInfectionStage.TumorFormed;
                infection.NextStageAt = _timing.CurTime + infection.TumorToAdvancedTime;
                SpawnTumorOrgan(uid);
                Dirty(uid, infection);
                break;

            case ZombieTumorInfectionStage.TumorFormed:
                // Advance to advanced stage
                infection.Stage = ZombieTumorInfectionStage.Advanced;
                Dirty(uid, infection);
                break;

            case ZombieTumorInfectionStage.Advanced:
                // Already at final stage
                break;
        }
    }

    /// <summary>
    /// Spawns a zombie tumor organ in the entity's torso. Returns true if successful.
    /// </summary>
    public bool SpawnTumorOrgan(EntityUid bodyUid)
    {
        // Find torso body part
        if (!TryComp<BodyComponent>(bodyUid, out var body))
            return false;

        var torso = _bodySystem.GetBodyChildrenOfType(bodyUid, BodyPartType.Torso, body).FirstOrDefault();
        if (torso.Id == EntityUid.Invalid)
            return false;

        // Try to add to first valid slot first (in case slot already exists)
        if (_bodySystem.CanInsertOrgan(torso.Id, "tumor", torso.Component))
        {
            // Slot exists, spawn and insert directly
            var tumorOrgan = Spawn("ZombieTumorOrgan", Transform(bodyUid).Coordinates);
            if (_bodySystem.InsertOrgan(torso.Id, tumorOrgan, "tumor", torso.Component, null))
            {
                return true;
            }
            // Insert failed, delete and try creating slot
            Del(tumorOrgan);
        }
        
        // If no slot available, try to create one
        if (_bodySystem.TryCreateOrganSlot(torso.Id, "tumor", out var slot))
        {
            // Spawn tumor organ after slot is created
            var tumorOrgan = Spawn("ZombieTumorOrgan", Transform(bodyUid).Coordinates);
            
            // Verify slot exists before inserting
            if (!_bodySystem.CanInsertOrgan(torso.Id, "tumor", torso.Component))
            {
                Log.Warning($"Tumor slot was created but is not available for insertion in {ToPrettyString(bodyUid)}");
                Del(tumorOrgan);
                return false;
            }
            
            // InsertOrgan returns false if it fails - check the result
            if (!_bodySystem.InsertOrgan(torso.Id, tumorOrgan, "tumor", torso.Component, null))
            {
                Log.Warning($"Failed to insert zombie tumor organ into {ToPrettyString(bodyUid)} after creating slot");
                Del(tumorOrgan);
                return false;
            }
            
            return true;
        }
        
        // If we get here, we couldn't add the tumor organ
        // Add the spawner component to retry later
        Log.Warning($"Could not create tumor slot for zombie tumor organ in {ToPrettyString(bodyUid)}, adding spawner component to retry");
        EnsureComp<ZombieTumorSpawnerComponent>(bodyUid);
        return false;
    }

    /// <summary>
    /// Checks if an entity already has a zombie tumor organ.
    /// </summary>
    public bool HasTumorOrgan(EntityUid bodyUid)
    {
        if (!TryComp<BodyComponent>(bodyUid, out var body))
            return false;

        var bodyEntity = (bodyUid, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        return tumorOrgans.Any();
    }

    private void UpdateOrganInfectionSpread(TimeSpan curTime)
    {
        _tumorJob.Organs.Clear();
        _tumorJob.Receivers.Clear();

        var organQuery = EntityQueryEnumerator<ZombieTumorOrganComponent, OrganComponent, TransformComponent>();
        while (organQuery.MoveNext(out var uid, out var organ, out var organComp, out var xform))
        {
            // Skip if not in a body
            if (organComp.Body == null)
                continue;

            // Check if it's time to update
            if (organ.NextUpdate > curTime)
                continue;

            organ.NextUpdate = curTime + organ.UpdateInterval;
            Dirty(uid, organ);

            _tumorJob.Organs.Add((uid, organ, xform));
            _tumorJob.Receivers.Add(new HashSet<Entity<MobStateComponent>>());
        }

        if (_tumorJob.Organs.Count == 0)
            return;

        // Process in parallel
        _parallel.ProcessNow(_tumorJob, _tumorJob.Organs.Count);

        // Collect all potential targets and count organs in range for each
        var targetOrganCounts = new Dictionary<EntityUid, int>();
        
        for (var i = 0; i < _tumorJob.Organs.Count; i++)
        {
            var receivers = _tumorJob.Receivers[i];
            
            foreach (var receiver in receivers)
            {
                if (Deleted(receiver) || _mobState.IsDead(receiver))
                    continue;

                if (HasComp<ZombieImmuneComponent>(receiver.Owner) || HasComp<ZombieTumorInfectionComponent>(receiver.Owner))
                    continue;

                // Count organs for this target
                if (!targetOrganCounts.ContainsKey(receiver.Owner))
                {
                    targetOrganCounts[receiver.Owner] = 0;
                }
                targetOrganCounts[receiver.Owner]++;
            }
        }

        // Apply infection chances based on organ count
        foreach (var (targetUid, count) in targetOrganCounts)
        {
            // Get base chance from first organ (they should all have the same chance)
            if (_tumorJob.Organs.Count == 0)
                continue;

            var (_, organ, _) = _tumorJob.Organs[0];
            var chance = organ.BaseInfectionChance * count;
            
            if (_random.Prob(chance))
            {
                InfectEntity(targetUid);
            }
        }
    }

    /// <summary>
    /// Infects an entity with the zombie tumor infection. This will progress through stages and eventually form a tumor organ.
    /// </summary>
    public void InfectEntity(EntityUid target)
    {
        if (HasComp<ZombieTumorInfectionComponent>(target))
            return;

        var infection = EnsureComp<ZombieTumorInfectionComponent>(target);
        infection.NextTick = _timing.CurTime + TimeSpan.FromSeconds(1);
        infection.NextStageAt = _timing.CurTime + infection.EarlyToTumorTime;
        Dirty(target, infection);

        _popup.PopupEntity(Loc.GetString("zombie-tumor-infection-contracted"), target, target);
    }

    private void OnInfectionMobStateChanged(Entity<ZombieTumorInfectionComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            // Zombify on death - the tumor organ will remain and continue spreading infection
            _zombieSystem.ZombifyEntity(ent.Owner);
            
            // Remove infection component since zombification is complete
            // The tumor organ itself remains and continues to spread infection
            RemComp<ZombieTumorInfectionComponent>(ent.Owner);
        }
    }

    private void OnInfectionRemoved(Entity<ZombieTumorInfectionComponent> ent, ref ComponentRemove args)
    {
        // Only remove tumor organ if infection is being removed while still alive
        // (e.g., via surgery). If zombified, the organ should remain.
        if (HasComp<ZombieComponent>(ent.Owner))
            return;

        // Cleanup: remove tumor organ if it exists
        if (!TryComp<BodyComponent>(ent, out var body))
            return;

        var bodyEntity = (ent.Owner, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        foreach (var (organUid, _, _) in tumorOrgans)
        {
            _bodySystem.RemoveOrgan(organUid);
            Del(organUid);
        }
    }

    private void OnTumorOrganRemoved(Entity<ZombieTumorOrganComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        // When tumor is removed, cure the infection
        if (args.OldBody != EntityUid.Invalid && HasComp<ZombieTumorInfectionComponent>(args.OldBody))
        {
            RemComp<ZombieTumorInfectionComponent>(args.OldBody);
            _popup.PopupEntity(Loc.GetString("zombie-tumor-removed"), args.OldBody, args.OldBody);
        }
    }

    private void OnTumorSpawnerInit(Entity<ZombieTumorSpawnerComponent> ent, ref ComponentInit args)
    {
        // Check immediately on init - body might be ready now
        if (HasTumorOrgan(ent.Owner))
        {
            RemComp<ZombieTumorSpawnerComponent>(ent.Owner);
            return;
        }

        // Try to spawn immediately
        if (SpawnTumorOrgan(ent.Owner))
        {
            RemComp<ZombieTumorSpawnerComponent>(ent.Owner);
        }
    }

    private void UpdateTumorSpawners(TimeSpan curTime)
    {
        // Check every 2 seconds (same as organ update interval)
        var spawnerQuery = EntityQueryEnumerator<ZombieTumorSpawnerComponent>();
        while (spawnerQuery.MoveNext(out var uid, out var spawner))
        {
            // Check if it's time to attempt again
            if (spawner.NextAttempt > curTime)
                continue;

            // Update next attempt time (retry every 2 seconds)
            spawner.NextAttempt = curTime + TimeSpan.FromSeconds(2);
            Dirty(uid, spawner);

            // Check if tumor already exists (maybe it was added another way)
            if (HasTumorOrgan(uid))
            {
                RemComp<ZombieTumorSpawnerComponent>(uid);
                continue;
            }

            // Try to spawn the tumor organ
            if (SpawnTumorOrgan(uid))
            {
                // Success! Remove the spawner component
                RemComp<ZombieTumorSpawnerComponent>(uid);
            }
            // If it fails, keep the component and try again next update
        }
    }

    private record struct TumorOrganJob(EntityLookupSystem Lookup, SharedTransformSystem Transform) : IParallelRobustJob
    {
        public int BatchSize => 8;

        public ValueList<(EntityUid Uid, ZombieTumorOrganComponent Organ, TransformComponent Xform)> Organs = new();
        public ValueList<HashSet<Entity<MobStateComponent>>> Receivers = new();

        public void Execute(int index)
        {
            var (_, organ, xform) = Organs[index];
            var receivers = Receivers[index];
            receivers.Clear();

            Lookup.GetEntitiesInRange(xform.Coordinates, organ.InfectionRange, receivers);
        }
    }
}
