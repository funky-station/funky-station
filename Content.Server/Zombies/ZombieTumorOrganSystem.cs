// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;
using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared.Actions;
using Content.Shared.Atmos;
using Content.Shared.Body.Components;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Shared.Chat;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Mobs;
using Robust.Shared.Map;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Zombies;
using Content.Shared._Shitmed.Targeting;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Robust.Shared.Collections;
using Robust.Shared.Map.Components;
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
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly ZombieSystem _zombieSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;
    [Dependency] private readonly InternalsSystem _internalsSystem = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private readonly TimeSpan _updateInterval = TimeSpan.FromSeconds(1); // Check every second for distance-based infection

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZombieTumorInfectionComponent, MobStateChangedEvent>(OnInfectionMobStateChanged);
        SubscribeLocalEvent<ZombieTumorInfectionComponent, ComponentRemove>(OnInfectionRemoved);
        SubscribeLocalEvent<ZombieTumorInfectionComponent, BeingGibbedEvent>(OnEntityWithInfectionBeingGibbed);
        SubscribeLocalEvent<BodyComponent, BeingGibbedEvent>(OnBodyBeingGibbed);
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

        // Heal zombie IPCs with robot tumors
        UpdateZombieIPChealing(curTime);
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

            // Skip damage if entity is already a zombie
            if (HasComp<ZombieComponent>(uid))
            {
                // Still check stage progression (though it shouldn't matter for zombies)
                if (infection.NextStageAt <= curTime)
                {
                    ProgressInfectionStage(uid, infection);
                }
                continue;
            }

            // Check if IPC
            if (HasComp<SiliconComponent>(uid) && TryComp<BloodstreamComponent>(uid, out var bloodstream))
            {
                HandleIPCDamage(uid, infection, damageable, bloodstream);
            }
            else
            {
                HandleNormalDamage(uid, infection, damageable, mobState);
            }

            // Handle sickness effects during TumorFormed and Advanced stages
            if (infection.Stage == ZombieTumorInfectionStage.TumorFormed || infection.Stage == ZombieTumorInfectionStage.Advanced)
            {
                HandleSicknessEffects(uid, infection, curTime);
            }

            // Handle Advanced stage ability and auto-zombify timers
            if (infection.Stage == ZombieTumorInfectionStage.Advanced)
            {
                // Give zombify self ability after 5 minutes in Advanced stage
                if (infection.ZombifySelfAbilityTime != null && infection.ZombifySelfAbilityTime <= curTime && !HasComp<IncurableZombieComponent>(uid))
                {
                    var incurable = EnsureComp<IncurableZombieComponent>(uid);
                    _actions.AddAction(uid, ref incurable.Action, incurable.ZombifySelfActionPrototype);
                    _popup.PopupEntity(Loc.GetString("zombie-tumor-ability-gained"), uid, uid);
                    
                    // Set auto-zombify timer for 5 minutes from now
                    infection.AutoZombifyTime = curTime + TimeSpan.FromMinutes(5);
                    Dirty(uid, infection);
                }

                // Auto-zombify after 5 minutes if they haven't used the ability
                if (infection.AutoZombifyTime != null && infection.AutoZombifyTime <= curTime)
                {
                    _zombieSystem.ZombifyEntity(uid);
                    RemComp<ZombieTumorInfectionComponent>(uid);
                    return;
                }
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
        // No damage during incubation period
        if (infection.Stage == ZombieTumorInfectionStage.Incubation)
            return;

        // Determine oil drain amount based on infection stage
        float drainPerTick;
        switch (infection.Stage)
        {
            case ZombieTumorInfectionStage.Early:
                drainPerTick = infection.EarlyOilDrain;
                break;
            case ZombieTumorInfectionStage.TumorFormed:
                drainPerTick = infection.TumorOilDrain;
                break;
            case ZombieTumorInfectionStage.Advanced:
                drainPerTick = infection.AdvancedOilDrain;
                break;
            default:
                return;
        }

        // Use bloodstream system to drain oil - same as how bleeding works
        // Check current blood level percentage
        var currentBloodLevel = _bloodstream.GetBloodLevelPercentage(uid, bloodstream);
        
        if (currentBloodLevel > 0)
        {
            // Drain oil using the bloodstream system (negative amount = removal)
            _bloodstream.TryModifyBloodLevel(uid, -drainPerTick, bloodstream);
        }
        else
        {
            // Oil is empty, apply radiation damage to torso only
            var multiplier = _mobState.IsCritical(uid) ? infection.CritDamageMultiplier : 1f;
            var damage = infection.RadiationDamage * multiplier;
            _damageable.TryChangeDamage(uid, damage, true, false, damageable, targetPart: TargetBodyPart.Torso);
        }
    }

    private void HandleNormalDamage(EntityUid uid, ZombieTumorInfectionComponent infection, DamageableComponent damageable, MobStateComponent mobState)
    {
        DamageSpecifier damage;
        switch (infection.Stage)
        {
            case ZombieTumorInfectionStage.Incubation:
                // No damage during incubation period
                return;
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
        _damageable.TryChangeDamage(uid, damage * multiplier, true, false, damageable, targetPart: TargetBodyPart.Torso);
    }

    private void HandleSicknessEffects(EntityUid uid, ZombieTumorInfectionComponent infection, TimeSpan curTime)
    {
        // Check if this entity has a RoboTumor (is an IPC)
        var hasRoboTumor = HasRoboTumor(uid);
        
        // Handle random messages based on stage
        if (infection.NextSicknessMessage <= curTime)
        {
            // Schedule next message at a random interval between 30-90 seconds
            infection.NextSicknessMessage = curTime + TimeSpan.FromSeconds(_random.Next(30, 91));
            
            string message;
            if (infection.Stage == ZombieTumorInfectionStage.Advanced)
            {
                // Advanced stage: paranoid/angry messages (or IPC error messages)
                var advancedMessages = hasRoboTumor ? new[]
                {
                    "zombie-robotumor-advanced-1",
                    "zombie-robotumor-advanced-2"
                } : new[]
                {
                    "zombie-tumor-advanced-1",
                    "zombie-tumor-advanced-2"
                };
                message = _random.Pick(advancedMessages);
            }
            else
            {
                // TumorFormed stage: sickness messages (or IPC malfunction messages)
                var sicknessMessages = hasRoboTumor ? new[]
                {
                    "zombie-robotumor-sickness-1",
                    "zombie-robotumor-sickness-2",
                    "zombie-robotumor-sickness-3"
                } : new[]
                {
                    "zombie-tumor-sickness-1",
                    "zombie-tumor-sickness-2",
                    "zombie-tumor-sickness-3",
                    "zombie-tumor-sickness-4",
                    "zombie-tumor-sickness-5"
                };
                message = _random.Pick(sicknessMessages);
            }
            
            _popup.PopupEntity(Loc.GetString(message), uid, uid);
        }

        // Handle random coughing/beeping
        if (infection.Stage == ZombieTumorInfectionStage.TumorFormed && infection.NextCough <= curTime)
        {
            // Schedule next cough/beep at a random interval between 15-45 seconds
            infection.NextCough = curTime + TimeSpan.FromSeconds(_random.Next(15, 46));
            
            // Make the entity cough (organics) or beep (IPCs)
            if (hasRoboTumor)
            {
                _chat.TryEmoteWithChat(uid, "Beep", ChatTransmitRange.Normal, ignoreActionBlocker: true);
            }
            else
            {
                _chat.TryEmoteWithChat(uid, "Cough", ChatTransmitRange.Normal, ignoreActionBlocker: true);
            }
        }
    }

    private void ProgressInfectionStage(EntityUid uid, ZombieTumorInfectionComponent infection)
    {
        switch (infection.Stage)
        {
            case ZombieTumorInfectionStage.Incubation:
                // Progress to early stage - symptoms begin and tumor forms immediately
                infection.Stage = ZombieTumorInfectionStage.Early;
                infection.NextStageAt = _timing.CurTime + infection.EarlyToTumorTime;
                SpawnTumorOrgan(uid);
                Dirty(uid, infection);
                
                // Use IPC-specific message if this entity has a RoboTumor
                var symptomsMessage = HasRoboTumor(uid) ? "zombie-robotumor-infection-symptoms-start" : "zombie-tumor-infection-symptoms-start";
                _popup.PopupEntity(Loc.GetString(symptomsMessage), uid, uid);
                break;

            case ZombieTumorInfectionStage.Early:
                // Progress to tumor formed stage (tumor already exists)
                infection.Stage = ZombieTumorInfectionStage.TumorFormed;
                infection.NextStageAt = _timing.CurTime + infection.TumorToAdvancedTime;
                
                // Initialize random timers for sickness effects
                infection.NextSicknessMessage = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(30, 91));
                infection.NextCough = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(15, 46));
                
                Dirty(uid, infection);
                break;

            case ZombieTumorInfectionStage.TumorFormed:
                // Advance to advanced stage - start converting blood to zombie blood
                infection.Stage = ZombieTumorInfectionStage.Advanced;
                
                // Initialize timer for paranoid/angry messages
                infection.NextSicknessMessage = _timing.CurTime + TimeSpan.FromSeconds(_random.Next(30, 91));
                
                // Set timer to give zombify self ability after 5 minutes
                infection.ZombifySelfAbilityTime = _timing.CurTime + TimeSpan.FromMinutes(5);
                
                Dirty(uid, infection);
                
                // Start converting blood to zombie blood (but not for IPCs - they keep their Oil)
                // Only change blood if NOT an IPC (doesn't have Silicon component)
                if (!HasComp<SiliconComponent>(uid) && TryComp<BloodstreamComponent>(uid, out var bloodstream))
                {
                    _bloodstream.ChangeBloodReagent(uid, "ZombieBlood", bloodstream);
                }
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
        // Check if entity has a body component
        if (!TryComp<BodyComponent>(bodyUid, out var body))
            return false;

        // Check if body has a root part (if no root part, body has no body parts/organs)
        if (body.RootContainer.ContainedEntity == null)
            return false;

        // Don't spawn if tumor already exists (prevents duplicate tumors)
        if (HasTumorOrgan(bodyUid))
            return true;

        // Find torso body part
        var torso = _bodySystem.GetBodyChildrenOfType(bodyUid, BodyPartType.Torso, body).FirstOrDefault();
        if (torso.Id == EntityUid.Invalid)
            return false;

        // Check if this is an IPC (has Silicon component AND Bloodstream - to exclude borgs)
        var isIPC = HasComp<SiliconComponent>(bodyUid) && HasComp<BloodstreamComponent>(bodyUid);
        var tumorPrototype = isIPC ? "ZombieRoboTumor" : "ZombieTumorOrgan";

        // Try to add to first valid slot first (in case slot already exists)
        if (_bodySystem.CanInsertOrgan(torso.Id, "tumor", torso.Component))
        {
            // Slot exists, spawn and insert directly
            // Spawn at map null to avoid state sync issues
            var tumorOrgan = Spawn(tumorPrototype, MapCoordinates.Nullspace);
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
            // Spawn at map null to avoid state sync issues
            var tumorOrgan = Spawn(tumorPrototype, MapCoordinates.Nullspace);
            
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

        // Check if body has a root part (if no root part, body has no body parts/organs)
        if (body.RootContainer.ContainedEntity == null)
            return false;

        var bodyEntity = (bodyUid, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        return tumorOrgans.Any();
    }

    private bool HasRoboTumor(EntityUid bodyUid)
    {
        if (!TryComp<BodyComponent>(bodyUid, out var body))
            return false;

        // Check if body has a root part (if no root part, body has no body parts/organs)
        if (body.RootContainer.ContainedEntity == null)
            return false;

        var bodyEntity = (bodyUid, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        
        // Check if any of the tumor organs is a ZombieRoboTumor prototype
        foreach (var (organUid, _, _) in tumorOrgans)
        {
            if (MetaData(organUid).EntityPrototype?.ID == "ZombieRoboTumor")
                return true;
        }
        
        return false;
    }

    private void UpdateOrganInfectionSpread(TimeSpan curTime)
    {
        // Track cumulative infection chance for each target from all tumors
        var targetInfectionChances = new Dictionary<EntityUid, float>();

        var organQuery = EntityQueryEnumerator<ZombieTumorOrganComponent, OrganComponent, TransformComponent>();
        while (organQuery.MoveNext(out var uid, out var organ, out var organComp, out var xform))
        {
            // Skip if not in a body
            if (organComp.Body == null)
                continue;

            // Check if it's time to update (now every 1 second)
            if (organ.NextUpdate > curTime)
                continue;

            organ.NextUpdate = curTime + organ.UpdateInterval;
            Dirty(uid, organ);

            // Get the body's transform (the zombie carrying the tumor)
            if (!TryComp(organComp.Body.Value, out TransformComponent? bodyXform))
                continue;

            // Use tile-based flood fill to find infectible entities in the same room with distances
            var infectableEntities = GetInfectableEntitiesInRoomWithDistances(organComp.Body.Value, bodyXform, organ.InfectionRange);

            // Calculate infection chance from this specific tumor based on its distance to each target
            foreach (var (target, distance) in infectableEntities)
            {
                // Calculate base infection chance based on THIS tumor's distance to the target
                float baseChance;
                if (distance <= 1f)
                    baseChance = 0.0035f; // 0.35% at 1 tile. 10% chance to infect at 1 tile in 30 sec.
                else if (distance <= 2f)
                    baseChance = 0.0017f; // 0.17% at 2 tiles. 5% chance to infect at 2 tiles in 30 sec.
                else if (distance <= 3f)
                    baseChance = 0.00067f; // 0.067% at 3 tiles. 2% chance to infect at 3 tiles in 30 sec.
                else
                    continue; // Beyond 3 tiles, no infection from this tumor

                // Add this tumor's infection chance to the target's cumulative chance
                if (!targetInfectionChances.ContainsKey(target))
                    targetInfectionChances[target] = 0f;
                
                targetInfectionChances[target] += baseChance;
            }
        }

        // Apply infection chances with protection modifiers
        foreach (var (targetUid, cumulativeChance) in targetInfectionChances)
        {
            // Apply protection modifiers
            var protectionMultiplier = GetProtectionMultiplier(targetUid);
            var finalChance = cumulativeChance * protectionMultiplier;

            // Roll for infection
            if (_random.Prob(finalChance))
            {
                InfectEntity(targetUid);
            }
        }
    }

    /// <summary>
    /// Calculates the protection multiplier based on mask and internals status.
    /// Returns 0.0 if internals are active (100% protection),
    /// 0.1 if wearing a mask (90% reduction),
    /// or 1.0 if no protection.
    /// </summary>
    private float GetProtectionMultiplier(EntityUid target)
    {
        // Check if internals are active - provides 100% protection
        if (_internalsSystem.AreInternalsWorking(target))
            return 0f;

        // Check if wearing a mask in the mask slot - provides 90% reduction
        if (_inventorySystem.TryGetSlotEntity(target, "mask", out var _))
            return 0.1f;

        // No protection
        return 1f;
    }

    /// <summary>
    /// Uses a tile-based flood fill to find all infectible entities in the same room as the source with their distances.
    /// This respects walls and closed doors, preventing infection through solid barriers.
    /// Might be laggy if the search range is large, works pretty well for small ranges.
    /// </summary>
    private Dictionary<EntityUid, float> GetInfectableEntitiesInRoomWithDistances(EntityUid sourceUid, TransformComponent sourceXform, float range)
    {
        var infectableEntities = new Dictionary<EntityUid, float>();
        var sourceMapPos = _transform.GetMapCoordinates(sourceUid);

        // Must be on a grid to use tile-based pathfinding
        if (!TryComp<MapGridComponent>(sourceXform.GridUid, out var grid))
        {
            // Fallback to simple range check if not on a grid (e.g., in space)
            var fallbackEntities = new HashSet<Entity<MobStateComponent>>();
            _entityLookup.GetEntitiesInRange(sourceXform.Coordinates, range, fallbackEntities);
            
            foreach (var entity in fallbackEntities)
            {
                if (entity.Owner == sourceUid)
                    continue;
                    
                if (_mobState.IsDead(entity))
                    continue;

                // Check for existing infection, tumor immunity, or zombie immunity
                if (HasComp<ZombieTumorInfectionComponent>(entity.Owner) || HasComp<ZombieTumorImmuneComponent>(entity.Owner) || HasComp<ZombieImmuneComponent>(entity.Owner))
                    continue;

                // Calculate actual distance
                var targetMapPos = _transform.GetMapCoordinates(entity.Owner);
                var distance = (targetMapPos.Position - sourceMapPos.Position).Length();
                infectableEntities[entity.Owner] = distance;
            }
            
            return infectableEntities;
        }

        var startTile = _mapSystem.TileIndicesFor(sourceXform.GridUid.Value, grid, sourceXform.Coordinates);
        var visited = new HashSet<Vector2i>();
        var queue = new Queue<Vector2i>();
        
        queue.Enqueue(startTile);
        visited.Add(startTile);

        var directions = new[] { AtmosDirection.North, AtmosDirection.South, AtmosDirection.East, AtmosDirection.West };
        var offsets = new[] { new Vector2i(0, 1), new Vector2i(0, -1), new Vector2i(1, 0), new Vector2i(-1, 0) };

        // Breadth-first search through tiles
        while (queue.Count > 0)
        {
            var currentTile = queue.Dequeue();

            // Calculate distance from start tile
            var tileDistance = (currentTile - startTile).Length;

            // Get all entities on this tile
            var tileEntities = _mapSystem.GetAnchoredEntitiesEnumerator(sourceXform.GridUid.Value, grid, currentTile);
            while (tileEntities.MoveNext(out var ent))
            {
                if (ent == sourceUid)
                    continue;

                // Check if this entity is a valid infection target
                if (!TryComp<MobStateComponent>(ent, out var mobState))
                    continue;

                if (_mobState.IsDead(ent.Value, mobState))
                    continue;

                // ZombieImmune doesn't protect against tumor infection (only ZombieTumorImmune does)
                if (HasComp<ZombieTumorInfectionComponent>(ent) || HasComp<ZombieTumorImmuneComponent>(ent))
                    continue;

                // Calculate actual world distance
                var targetMapPos = _transform.GetMapCoordinates(ent.Value);
                var actualDistance = (targetMapPos.Position - sourceMapPos.Position).Length();
                
                // Use the closest distance if entity already found
                if (!infectableEntities.ContainsKey(ent.Value))
                    infectableEntities[ent.Value] = actualDistance;
                else
                    infectableEntities[ent.Value] = Math.Min(infectableEntities[ent.Value], actualDistance);
            }

            // Also check non-anchored entities on this tile
            var worldPos = _mapSystem.GridTileToWorld(sourceXform.GridUid.Value, grid, currentTile);
            var nonAnchoredEntities = new HashSet<Entity<MobStateComponent>>();
            _entityLookup.GetEntitiesInRange(worldPos, 0.5f, nonAnchoredEntities); // Half tile radius
            
            foreach (var entity in nonAnchoredEntities)
            {
                if (entity.Owner == sourceUid)
                    continue;

                if (_mobState.IsDead(entity))
                    continue;

                // Check for existing infection, tumor immunity, or zombie immunity
                if (HasComp<ZombieTumorInfectionComponent>(entity.Owner) || HasComp<ZombieTumorImmuneComponent>(entity.Owner) || HasComp<ZombieImmuneComponent>(entity.Owner))
                    continue;

                // Calculate actual world distance
                var targetMapPos = _transform.GetMapCoordinates(entity.Owner);
                var actualDistance = (targetMapPos.Position - sourceMapPos.Position).Length();
                
                // Use the closest distance if entity already found
                if (!infectableEntities.ContainsKey(entity.Owner))
                    infectableEntities[entity.Owner] = actualDistance;
                else
                    infectableEntities[entity.Owner] = Math.Min(infectableEntities[entity.Owner], actualDistance);
            }

            // Check adjacent tiles
            for (int i = 0; i < directions.Length; i++)
            {
                var direction = directions[i];
                var offset = offsets[i];
                var neighborTile = currentTile + offset;

                if (visited.Contains(neighborTile))
                    continue;

                // Check if blocked by walls/closed doors using atmos system
                if (_atmosphereSystem.IsTileAirBlocked(sourceXform.GridUid.Value, currentTile, direction, grid))
                    continue;

                // Check distance from start
                var neighborDistance = (neighborTile - startTile).Length;
                if (neighborDistance > range)
                    continue;

                queue.Enqueue(neighborTile);
                visited.Add(neighborTile);
            }
        }

        return infectableEntities;
    }

    /// <summary>
    /// Infects an entity with the zombie tumor infection. This will progress through stages and eventually form a tumor organ.
    /// </summary>
    public void InfectEntity(EntityUid target)
    {
        if (HasComp<ZombieTumorInfectionComponent>(target))
            return;

        // Don't infect if immune to tumor infection (from Ambuzol Plus)
        if (HasComp<ZombieTumorImmuneComponent>(target))
            return;

        // Only infect entities that have bodies with organs (tumor requires a body structure)
        if (!TryComp<BodyComponent>(target, out var body) || body.RootContainer.ContainedEntity == null)
            return;

        // Don't infect entities without a bloodstream (borgs, etc)
        if (!HasComp<BloodstreamComponent>(target))
            return;

        var infection = EnsureComp<ZombieTumorInfectionComponent>(target);
        infection.Stage = ZombieTumorInfectionStage.Incubation;
        infection.NextTick = _timing.CurTime + TimeSpan.FromSeconds(1);
        infection.NextStageAt = _timing.CurTime + infection.IncubationToEarlyTime;
        Dirty(target, infection);

        // Use IPC-specific message if this is an IPC (has Silicon component AND Bloodstream)
        var isIPC = HasComp<SiliconComponent>(target) && HasComp<BloodstreamComponent>(target);
        var message = isIPC ? "zombie-robotumor-infection-contracted" : "zombie-tumor-infection-contracted";
        _popup.PopupEntity(Loc.GetString(message), target, target);
    }

    private void OnInfectionMobStateChanged(Entity<ZombieTumorInfectionComponent> ent, ref MobStateChangedEvent args)
    {
        // Check if this is an IPC (has Silicon component AND Bloodstream - to exclude borgs)
        var isIPC = HasComp<SiliconComponent>(ent.Owner) && HasComp<BloodstreamComponent>(ent.Owner);

        // IPCs zombify when they become critical OR dead
        // Note: IPCs have a critical threshold at 119.999 and dead at 120, so with 1.0 damage ticks
        // they often skip critical entirely and go straight to dead. We handle both states for IPCs.
        // Organic entities zombify only on death
        if ((isIPC && (args.NewMobState == MobState.Critical || args.NewMobState == MobState.Dead)) || 
            (!isIPC && args.NewMobState == MobState.Dead))
        {
            // Ensure tumor is spawned before zombifying (important for IPCs so they get the correct RoboTumor type)
            // This checks blood type, so must happen before ZombifyEntity changes it
            if (!HasTumorOrgan(ent.Owner))
            {
                SpawnTumorOrgan(ent.Owner);
            }
            
            // Zombify - the tumor organ will remain and continue spreading infection
            _zombieSystem.ZombifyEntity(ent.Owner);
            
            // Remove infection component since zombification is complete
            // The tumor organ itself remains and continues to spread infection
            RemComp<ZombieTumorInfectionComponent>(ent.Owner);
        }
    }

    /// <summary>
    /// Public method to handle zombie death infection spread. Called by ZombieSystem.
    /// </summary>
    public void HandleZombieDeathInfection(EntityUid zombieUid)
    {
        // Perform a one-time infection spread with higher chances
        if (!TryComp(zombieUid, out TransformComponent? xform))
            return;

        // Get all infectible entities in the same room with distances
        var infectableEntities = GetInfectableEntitiesInRoomWithDistances(zombieUid, xform, 3f);

        // Apply death infection chances based on distance
        foreach (var (targetUid, distance) in infectableEntities)
        {
            float infectionChance;
            if (distance <= 1f)
                infectionChance = 0.5f; // 50% at 1 tile
            else if (distance <= 2f)
                infectionChance = 0.25f; // 25% at 2 tiles
            else if (distance <= 3f)
                infectionChance = 0.1f; // 10% at 3 tiles
            else
                continue; // Beyond 3 tiles, no infection

            // Apply protection modifiers (mask/internals still work)
            var protectionMultiplier = GetProtectionMultiplier(targetUid);
            infectionChance *= protectionMultiplier;

            // Roll for infection
            if (_random.Prob(infectionChance))
            {
                InfectEntity(targetUid);
            }
        }
    }

    private void OnInfectionRemoved(Entity<ZombieTumorInfectionComponent> ent, ref ComponentRemove args)
    {
        // Skip if entity is being deleted (prevents crashes during entity deletion)
        if (TerminatingOrDeleted(ent.Owner))
            return;

        // If zombified, the organ should remain. How someone has an infection removed while zombified is questionable.
        if (HasComp<ZombieComponent>(ent.Owner))
            return;

        // Cleanup: remove tumor organ if it exists
        if (!TryComp<BodyComponent>(ent, out var body))
            return;

        // Check if body has a root part (if no root part, body has no body parts/organs)
        // Redundant check, but just in case.
        if (body.RootContainer.ContainedEntity == null)
            return;

        var bodyEntity = (ent.Owner, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        foreach (var (organUid, _, _) in tumorOrgans)
        {
            // Skip if organ is already being deleted
            // Redundant check, but just incase.
            if (TerminatingOrDeleted(organUid))
                continue;

            // Remove the organ from the body - it will drop as an item
            // Deletion will be handled naturally if needed
            _bodySystem.RemoveOrgan(organUid);
        }
    }

    private void OnEntityWithInfectionBeingGibbed(Entity<ZombieTumorInfectionComponent> ent, ref BeingGibbedEvent args)
    {
        // Remove tumor organs before gibbing to prevent client-side state sync assertion failures
        // This prevents the "organ.Body == null" assertion error when rapidly deleting organs
        RemoveTumorOrgansBeforeGib(ent.Owner);
    }

    private void OnBodyBeingGibbed(Entity<BodyComponent> ent, ref BeingGibbedEvent args)
    {
        // Also handle bodies without infection component (e.g., zombies with tumors)
        RemoveTumorOrgansBeforeGib(ent.Owner);
    }

    private void RemoveTumorOrgansBeforeGib(EntityUid bodyUid)
    {
        if (!TryComp<BodyComponent>(bodyUid, out var body))
            return;

        if (body.RootContainer.ContainedEntity == null)
            return;

        var bodyEntity = (bodyUid, body);
        var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
        
        // Collect organs to remove first to avoid modifying collection during iteration
        var organsToRemove = new List<EntityUid>();
        foreach (var (organUid, _, _) in tumorOrgans)
        {
            if (!TerminatingOrDeleted(organUid))
                organsToRemove.Add(organUid);
        }

        // Remove and delete all tumor organs before gibbing
        foreach (var organUid in organsToRemove)
        {
            // RemoveOrgan will clear the Body reference internally
            _bodySystem.RemoveOrgan(organUid);
            // Immediate deletion to prevent state sync issues
            QueueDel(organUid);
        }
    }

    private void OnTumorOrganRemoved(Entity<ZombieTumorOrganComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        // Skip if entities are being deleted (prevents client-side crashes during PVS departure)
        if (TerminatingOrDeleted(ent.Owner) || args.OldBody == EntityUid.Invalid || TerminatingOrDeleted(args.OldBody))
            return;

        // When tumor is removed, cure the infection
        if (HasComp<ZombieTumorInfectionComponent>(args.OldBody))
        {
            // Check if removed tumor was a RoboTumor
            var wasRoboTumor = MetaData(ent.Owner).EntityPrototype?.ID == "ZombieRoboTumor";
            var removalMessage = wasRoboTumor ? "zombie-robotumor-removed" : "zombie-tumor-removed";
            
            RemComp<ZombieTumorInfectionComponent>(args.OldBody);
            _popup.PopupEntity(Loc.GetString(removalMessage), args.OldBody, args.OldBody);
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

    private void UpdateZombieIPChealing(TimeSpan curTime)
    {
        // Heal zombie IPCs that have robot tumors at the same rate as normal zombies
        var zombieQuery = EntityQueryEnumerator<ZombieComponent, DamageableComponent, MobStateComponent, SiliconComponent>();
        while (zombieQuery.MoveNext(out var uid, out var zombie, out var damageable, out var mobState, out var silicon))
        {
            // Only heal IPCs (those with Silicon component)
            // Already filtered by query

            // Check if they have a robot tumor organ
            if (!TryComp<BodyComponent>(uid, out var body) || body.RootContainer.ContainedEntity == null)
                continue;

            var bodyEntity = (uid, body);
            var tumorOrgans = _bodySystem.GetBodyOrganEntityComps<ZombieTumorOrganComponent>(bodyEntity);
            
            // Check if any tumor organ is a ZombieRoboTumor
            bool hasRoboTumor = false;
            foreach (var (organUid, _, _) in tumorOrgans)
            {
                if (MetaData(organUid).EntityPrototype?.ID == "ZombieRoboTumor")
                {
                    hasRoboTumor = true;
                    break;
                }
            }

            if (!hasRoboTumor)
                continue;

            // Process only once per second (same as normal zombie healing)
            if (zombie.NextTick + TimeSpan.FromSeconds(1) > curTime)
                continue;

            zombie.NextTick = curTime;
            Dirty(uid, zombie);

            if (_mobState.IsDead(uid, mobState))
                continue;

            // Use the same healing as normal zombies
            var multiplier = _mobState.IsCritical(uid, mobState)
                ? zombie.PassiveHealingCritMultiplier
                : 1f;

            // Heal the zombie IPC at the same rate as normal zombies
            _damageable.TryChangeDamage(uid, zombie.PassiveHealing * multiplier, true, false, damageable);
        }
    }
}
