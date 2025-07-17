// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 willowzeta <willowzeta632146@proton.me>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Heretic.Prototypes;
using Content.Shared.Changeling;
using Content.Shared.Mobs.Components;
using Content.Shared.Humanoid;
using Content.Server.Revolutionary.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Damage;
using Content.Shared.Heretic;
using Content.Server.Heretic.EntitySystems;
using Content.Server.Chat.Managers;
using Content.Server.Atmos.EntitySystems;
using Robust.Shared.Random;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Server.Body.Systems;
using Content.Shared.Inventory;
using Robust.Server.GameObjects;
using Content.Shared.Chat;
using System.Linq;
using Robust.Shared.Physics;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server.Heretic.Ritual;

/// <summary>
///     Checks for a nearest dead body,
///     gibs it and gives the heretic knowledge points.
///     no longer gibs it, just teleports -space
/// </summary>
// these classes should be lead out and shot
[Virtual]
public partial class RitualSacrificeBehavior : RitualCustomBehavior
{
    /// <summary>
    ///     Minimal amount of corpses.
    /// </summary>
    [DataField]
    public float Min = 1;

    /// <summary>
    ///     Maximum amount of corpses.
    /// </summary>
    [DataField]
    public float Max = 1;

    /// <summary>
    ///     Should we count only targets?
    /// </summary>
    [DataField]
    public bool OnlyTargets = false;

    protected List<EntityUid> uids = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        var lookupSystem = args.EntityManager.System<EntityLookupSystem>();

        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
        {
            outstr = string.Empty;
            return false;
        }

        var res = lookupSystem.GetEntitiesInRange(args.Platform, .75f);
        if (res.Count == 0 || res == null)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        // get all the dead ones
        foreach (var found in res)
        {
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(found, out var mobstate) // only mobs
            || !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(found) // only humans
            || OnlyTargets && !hereticComp.SacrificeTargets.Contains(args.EntityManager.GetNetEntity(found))) // only targets
                continue;

            if (mobstate.CurrentState == MobState.Dead)
                uids.Add(found);
        }

        if (uids.Count < Min)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice-ineligible");
            return false;
        }

        outstr = null;
        return true;
    }

    public override void Finalize(RitualData args)
    {
        var hereticSystem = args.EntityManager.System<HereticSystem>();
        var mindSystem = args.EntityManager.System<SharedMindSystem>();

        for (var i = 0; i < Max; i++)
        {
            if (args.EntityManager.HasComponent<SacrificedComponent>(uids[i]))
                continue;

            var isCommand = args.EntityManager.HasComponent<CommandStaffComponent>(uids[i]);
            var knowledgeGain = isCommand ? 6f : 4f; //funkystation: changed to encourage sacs

            // funkystation
            // start the sacrifing process -space
            if (args.EntityManager.TryGetComponent<TransformComponent>(uids[i], out var transform))
            {
                var uid = uids[i];
                SafeSacrifice(args, uid);
            }

            if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
                hereticSystem.UpdateKnowledge(args.Performer, hereticComp, knowledgeGain);

            // update objectives
            if (mindSystem.TryGetMind(args.Performer, out var mindId, out var mind))
            {
                // this is godawful dogshit. but it works :)
                if (mindSystem.TryFindObjective((mindId, mind), "HereticSacrificeObjective", out var crewObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp))
                    crewObjComp.Sacrificed += 1;

                if (mindSystem.TryFindObjective((mindId, mind), "HereticSacrificeHeadObjective", out var crewHeadObj)
                && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewHeadObj, out var crewHeadObjComp)
                && isCommand)
                    crewHeadObjComp.Sacrificed += 1;
            }
        }

        // reset it because it refuses to work otherwise.
        uids = new();
        args.EntityManager.EventBus.RaiseLocalEvent(args.Performer, new EventHereticUpdateTargets());
    }

    // funkystation
    // sacrifice function to safely teleport them away -space
    private void SafeSacrifice(RitualData args, EntityUid uid)
    {
        var transformSystem = args.EntityManager.System<TransformSystem>();
        var inventorySystem = args.EntityManager.System<InventorySystem>();
        var sharedMindSystem = args.EntityManager.System<SharedMindSystem>();
        var bloodSystem = args.EntityManager.System<BloodstreamSystem>();
        var mobStateSystem = args.EntityManager.System<MobStateSystem>();
        var damageSystem = args.EntityManager.System<DamageableSystem>();
        var chatManager = IoCManager.Resolve<IChatManager>();

        if (args.EntityManager.TryGetComponent<InventoryComponent>(uid, out var comp2))
        {
            foreach (var item in inventorySystem.GetHandOrInventoryEntities(uid))
            {
                transformSystem.DropNextTo(item, uid);
            }
        }

        if (!args.EntityManager.TryGetComponent<MobStateComponent>(uid, out var mobstate))
            return;

        if (mobstate.CurrentState == MobState.Dead)
        {
            // check if DEAD
            TeleportRandomly(args, uid); //send the loop over to a function -space

            // tell them they've been sacrificed -space
            var message = Loc.GetString("sacrificed-description");
            var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));

            if (message is not null &&
                sharedMindSystem.TryGetMind(uid, out _, out var mindComponent) &&
                mindComponent.Session != null)
            {
                chatManager.ChatMessageToOne(ChatChannel.Server,
                    message,
                    wrappedMessage,
                    default,
                    false,
                     mindComponent.Session.Channel,
                     Color.FromSrgb(new Color(255, 100, 255)));
            }

            args.EntityManager.EnsureComponent<SacrificedComponent>(uid);
        }

        // remove all damage -space

        if (!args.EntityManager.TryGetComponent<DamageableComponent>(uid, out var damageable))
            return;

        damageSystem.SetAllDamage(uid, damageable, 0);
        mobStateSystem.ChangeMobState(uid, MobState.Alive);
        bloodSystem.TryModifyBloodLevel(uid, 1000);
        bloodSystem.TryModifyBleedAmount(uid, -1000);

    }

    private void TeleportRandomly(RitualData args, EntityUid uid) // start le teleporting loop -space
    {
        var transformSystem = args.EntityManager.System<SharedTransformSystem>();
        var lookupSystem = args.EntityManager.System<EntityLookupSystem>();
        var xformSystem = args.EntityManager.System<TransformSystem>();
        var randomSystem = IoCManager.Resolve<IRobustRandom>();
        var sharedXformSystem = args.EntityManager.System<SharedTransformSystem>();
        var atmosSystem = args.EntityManager.System<AtmosphereSystem>();
        var pullSystem = args.EntityManager.System<PullingSystem>();

        var maxrandomtp = 50; // this is how many attempts it will try before breaking the loop -space
        var maxrandomradius = 40; // this is the max range it will do -space

        if (!args.EntityManager.TryGetComponent<TransformComponent>(uid, out var transformComponent))
            return;

        // Stop the heretic to being pulled with the sacrificed target (or anything else who is pulling it) -space
        if (args.EntityManager.TryGetComponent<PullableComponent>(uid, out var pull))
            pullSystem.TryStopPull(uid, pull);


        var coords = transformComponent.Coordinates;
        var newCoords = coords.Offset(randomSystem.NextVector2(maxrandomradius));
        for (var i = 0; i < maxrandomtp; i++) //start of the loop -space
        {
            var randVector = randomSystem.NextVector2(maxrandomradius);
            newCoords = coords.Offset(randVector);

            xformSystem.SetCoordinates(uid, newCoords); //move person teleported to check if they're under a tile (the tiles intersecting doesnt account for this so we need to do this) -space

            var air = atmosSystem.GetContainingMixture((uid, transformComponent)); //check if the room has any sort of atmos (this prevents getting teleported into solars and grilles outta the station) -space

            // if they're not in space and not in wall, it will choose these coords and end the loop -space
            if (transformComponent.GridUid != null && air != null && !lookupSystem.GetEntitiesIntersecting(newCoords.ToMap(args.EntityManager, sharedXformSystem), LookupFlags.Static).Any())
                break;
        }

    }
}
