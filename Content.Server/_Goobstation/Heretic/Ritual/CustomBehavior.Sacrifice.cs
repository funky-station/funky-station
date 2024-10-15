using Content.Shared.Heretic.Prototypes;
using Content.Shared.Changeling;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using Content.Shared.Humanoid;
using Content.Server.Revolutionary.Components;
using Content.Server.Objectives.Components;
using Content.Shared.Mind;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Heretic;
using Content.Server.Heretic.EntitySystems;
using JetBrains.FormatRipper.Elf;
using System.Numerics;
using System;
using Robust.Shared.Random;
using Content.Shared.Database;
using Robust.Shared.Audio;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Toolshed.TypeParsers;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Content.Server.Chat.Systems;
using Content.Server.Traits.Assorted;
using Content.Server.Body.Systems;
using Content.Server.EUI;
using Content.Server.Roles;
using Content.Server.Antag;
using Content.Shared.Inventory;
using static Content.Server.Power.Pow3r.PowerState;
using Content.Shared.Popups;
using Content.Server.Destructible;
using Robust.Server.GameObjects;
using Content.Shared.Chat;
using Content.Shared.Storage.Components;
using static Content.Shared.Administration.Notes.AdminMessageEuiState;
using Content.Server.Chat.Managers;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction;
using Robust.Shared.Network.Messages;
using System.Linq;
using YamlDotNet.Core.Tokens;
using FastAccessors;
using Content.Shared.Humanoid.Markings;
using JetBrains.Annotations;
using System.ComponentModel.Design;

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
    [DataField] public float Min = 1;

    /// <summary>
    ///     Maximum amount of corpses.
    /// </summary>
    [DataField] public float Max = 1;

    /// <summary>
    ///     Should we count only targets?
    /// </summary>
    [DataField] public bool OnlyTargets = false;

    // this is awful but it works so i'm not complaining
    // yeah its dogshit, i hate it -space
    protected SharedMindSystem _mind = default!;
    protected EntityManager _ent = default!;
    protected HereticSystem _heretic = default!;
    protected DamageableSystem _damage = default!;
    protected BloodstreamSystem _blood = default!;
    protected EntityLookupSystem _lookup = default!;
    protected SharedTransformSystem _xform = default!;
    protected AntagSelectionSystem _antag = default!;
    protected DestructibleSystem _system = default!;
    [Dependency] protected EuiManager _euiMan = default!;
    [Dependency] protected IPrototypeManager _proto = default!;
    [Dependency] protected IRobustRandom _random = default!;
    [Dependency] protected ISharedAdminLogManager _adminLogger = default!;
    [Dependency] protected IChatManager _chat = default!;
    protected MobStateSystem _mobState = default!;
    protected MobThresholdSystem _mobThreshold = default!;

    protected List<EntityUid> uids = new();

    public override bool Execute(RitualData args, out string? outstr)
    {
        _mind = args.EntityManager.System<SharedMindSystem>();
        _heretic = args.EntityManager.System<HereticSystem>();
        _damage = args.EntityManager.System<DamageableSystem>();
        _blood = args.EntityManager.System<BloodstreamSystem>();
        _lookup = args.EntityManager.System<EntityLookupSystem>();
        _xform = args.EntityManager.System<SharedTransformSystem>();
        _antag = args.EntityManager.System<AntagSelectionSystem>();
        _system = args.EntityManager.System<DestructibleSystem>();
        _ent = IoCManager.Resolve<EntityManager>();
        _euiMan = IoCManager.Resolve<EuiManager>();
        _random = IoCManager.Resolve<IRobustRandom>();
        _proto = IoCManager.Resolve<IPrototypeManager>();
        _adminLogger = IoCManager.Resolve<ISharedAdminLogManager>();
        _chat = IoCManager.Resolve<IChatManager>();
        _mobState = args.EntityManager.System<MobStateSystem>();
        _mobThreshold = args.EntityManager.System<MobThresholdSystem>();

        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
        {
            outstr = string.Empty;
            return false;
        }

        var lookup = _lookup.GetEntitiesInRange(args.Platform, .75f);
        if (lookup.Count == 0 || lookup == null)
        {
            outstr = Loc.GetString("heretic-ritual-fail-sacrifice");
            return false;
        }

        // get all the dead ones
        foreach (var look in lookup)
        {
            if (!args.EntityManager.TryGetComponent<MobStateComponent>(look, out var mobstate) // only mobs
            || !args.EntityManager.HasComponent<HumanoidAppearanceComponent>(look) // only humans
            || (OnlyTargets && !hereticComp.SacrificeTargets.Contains(args.EntityManager.GetNetEntity(look)))) // only targets
                continue;

            if (mobstate.CurrentState == Shared.Mobs.MobState.Dead)
                uids.Add(look);
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
        for (int i = 0; i < Max; i++)
        {
            var isCommand = args.EntityManager.HasComponent<CommandStaffComponent>(uids[i]);
            var knowledgeGain = isCommand ? 2f : 1f;

            // start the sacrifing process -space
            if (args.EntityManager.TryGetComponent<TransformComponent>(uids[i], out var transform))
            {

                //drop all items -space


                if (args.EntityManager.TryGetComponent<InventoryComponent>(uids[i], out var comp2))
                {
                    var transformSystem = _system.EntityManager.System<TransformSystem>();
                    var inventorySystem = _system.EntityManager.System<InventorySystem>();
                    var sharedPopupSystem = _system.EntityManager.System<SharedPopupSystem>();

                    foreach (var item in inventorySystem.GetHandOrInventoryEntities(uids[i]))
                    {
                        transformSystem.DropNextTo(item, uids[i]);
                    }
                }




                // teleports them away -space
                var mapPos = _xform.GetWorldPosition(transform);
                var radius = 30;
                var gridBounds = new Box2(mapPos - new Vector2(radius, radius), mapPos + new Vector2(radius, radius));

                var mobs = new HashSet<Entity<MobStateComponent>>();
                _lookup.GetEntitiesInRange(transform.Coordinates, .0000001f, mobs, flags: LookupFlags.Uncontained);
                foreach (var comp in mobs)
                {

                    if (!args.EntityManager.TryGetComponent<MobStateComponent>(comp, out var mobstate)) // only mobs
                        continue;

                    if (mobstate.CurrentState == Shared.Mobs.MobState.Dead)
                    {

                        // because im dogshit at coding, we just gotta target all the dead people so heretic doesnt get teleported -space
                        // will teleport other dead bodies away if there is any but im too lazy to figure out a better way -space

                        var ent = comp.Owner;
                        var randomX = _random.NextFloat(gridBounds.Left, gridBounds.Right);
                        var randomY = _random.NextFloat(gridBounds.Bottom, gridBounds.Top);

                        var pos = new Vector2(randomX, randomY);

                        _xform.SetWorldPosition(ent, pos);

                        // tell them they've been sacrificed -space

                        var message = "You wake up, a piece of your soul and some memories all missing. You can't remember any of the events that lead up towards whatever happened to you, and you especially can't remember anyone who did this to you.";
                        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", message));


                        if (message is not null &&
                            _mind.TryGetMind(ent, out _, out var mindComponent) &&
                            mindComponent.Session != null)
                        {

                            _chat.ChatMessageToOne(ChatChannel.Server,
                            message,
                            wrappedMessage,
                            default,
                            false,
                            mindComponent.Session.Channel,
                            Color.FromSrgb(new Color(255, 100, 255)));

                        }



                        //remove the fucking target -space

                        if (!args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp2))
                            return;

                        var sactargs = hereticComp2.SacrificeTargets;


                        foreach (var target in sactargs)
                        {
                            var ent2 = _ent.GetEntity(target);

                            if (ent2 == ent) ;
                            {
                                _ent.AddComponent<SacrificedComponent>(ent);
                            }
                        }

                    }

                }

                // remove all damage -space

                var uid = uids[i];
                if (!args.EntityManager.TryGetComponent<DamageableComponent>(uid, out var damageable))
                    return;


                _damage.SetAllDamage(uid, damageable, 0);
                _mobState.ChangeMobState(uid, MobState.Alive);
                _blood.TryModifyBloodLevel(uid, 1000);
                _blood.TryModifyBleedAmount(uid, -1000);

                // reset it because it refuses to work otherwise.
                uids = new();
                args.EntityManager.EventBus.RaiseLocalEvent(args.Performer, new EventHereticUpdateTargets());

                if (args.EntityManager.TryGetComponent<HereticComponent>(args.Performer, out var hereticComp))
                    _heretic.UpdateKnowledge(args.Performer, hereticComp, knowledgeGain);

                // update objectives
                if (_mind.TryGetMind(args.Performer, out var mindId, out var mind))
                {
                    // this is godawful dogshit. but it works :)
                    if (_mind.TryFindObjective((mindId, mind), "HereticSacrificeObjective", out var crewObj)
                    && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewObj, out var crewObjComp))
                        crewObjComp.Sacrificed += 1;

                    if (_mind.TryFindObjective((mindId, mind), "HereticSacrificeHeadObjective", out var crewHeadObj)
                    && args.EntityManager.TryGetComponent<HereticSacrificeConditionComponent>(crewHeadObj, out var crewHeadObjComp)
                    && isCommand)
                        crewHeadObjComp.Sacrificed += 1;
                }
            }

        }

        // reset it because it refuses to work otherwise.
        uids = new();
        args.EntityManager.EventBus.RaiseLocalEvent(args.Performer, new EventHereticUpdateTargets());
    }
}
