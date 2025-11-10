// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers@gmail.com>
// SPDX-FileCopyrightText: 2023 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <55817627+coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 coolmankid12345 <coolmankid12345@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Killerqu00 <47712032+Killerqu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 LordCarve <27449516+LordCarve@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Rainfey <rainfey0+github@gmail.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 silver2127 <52584484+silver2127@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2025 Rainbow <ev0lvkitten@gmail.com>
// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Logs;
using Content.Server.Antag;
using Content.Server.EUI;
using Content.Server.Flash;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Revolutionary;
using Content.Server.Revolutionary.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement;
using Content.Shared.Mind.Components;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Revolutionary.Components;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Content.Shared.Cuffs.Components;
using Content.Shared.Revolutionary;
using Content.Server.Communications;
using System.Linq;
using System.Threading;
using Content.Shared.Chat;
using Content.Server.Chat.Systems;
using Content.Server.PDA.Ringer;
using Content.Server.Traitor.Uplink;
using Content.Shared.Changeling;
using Content.Shared.Heretic;
using Content.Shared.Implants;
using Robust.Shared.Audio;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Where all the main stuff for Revolutionaries happens (Assigning Head Revs, Command on station, and checking for the game to end.)
/// </summary>
// Heavily edited by goobstation. If you want to upstream something think twice
public sealed class RevolutionaryRuleSystem : GameRuleSystem<RevolutionaryRuleComponent>
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;
    [Dependency] private readonly EuiManager _euiMan = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RoundEndSystem _roundEnd = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly SharedRevolutionarySystem _revolutionarySystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UplinkSystem _uplink = default!;


    //Used in OnPostFlash, no reference to the rule component is available
    public readonly ProtoId<NpcFactionPrototype> RevolutionaryNpcFaction = "Revolutionary";
    public readonly ProtoId<NpcFactionPrototype> RevPrototypeId = "Rev";

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CommandStaffComponent, MobStateChangedEvent>(OnCommandMobStateChanged);
        SubscribeLocalEvent<HeadRevolutionaryComponent, MobStateChangedEvent>(OnHeadRevMobStateChanged);
        SubscribeLocalEvent<HeadRevolutionaryComponent, DeclareOpenRevoltEvent>(OnHeadRevDeclareOpenRevolt); //Funky Station
        
        SubscribeLocalEvent<RevolutionaryLieutenantComponent, ImplantImplantedEvent>(OnLieutenantImplant); // Funky Station
        SubscribeLocalEvent<RevolutionaryRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected); // Funky Station
        SubscribeLocalEvent<RevolutionaryRoleComponent, GetBriefingEvent>(OnGetBriefing);
        SubscribeLocalEvent<HeadRevolutionaryComponent, AfterFlashedEvent>(OnPostFlash);
        SubscribeLocalEvent<ShuttleDockAttemptEvent>(OnTryShuttleDock); // Funky Station - HE- HE- HELL NAW
    }

    protected override void Started(EntityUid uid, RevolutionaryRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
        component.CommandCheck = _timing.CurTime + component.TimerWait;
    }

    private void AfterEntitySelected(Entity<RevolutionaryRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeHeadRevolutionary(args.EntityUid, ent);
    }
    
    // dont need checks for multiple implants since we alr disabled that
    private bool CanBeLieutenant(EntityUid uid)
    {
        return !HasComp<HeadRevolutionaryComponent>(uid) && HasComp<RevolutionaryComponent>(uid);
    }
    
    private void OnLieutenantImplant(Entity<RevolutionaryLieutenantComponent> component, ref ImplantImplantedEvent ev)
    {
        if (ev.Implanted == null)
            return;

        if (!CanBeLieutenant(ev.Implanted.Value)) 
            return;
        
        if (!_mind.TryGetMind(ev.Implanted.Value, out var mindId, out _))
            return;
        
        EnsureComp<RevolutionaryLieutenantComponent>(ev.Implanted.Value);
        _antag.SendBriefing(ev.Implanted.Value, Loc.GetString("rev-lieutenant-greeting"), Color.Red, new SoundPathSpecifier("/Audio/_Funkystation/Ambience/Antag/Revolutionary/rev_lieu_intro.ogg"));
        
        if (_role.MindHasRole<RevolutionaryRoleComponent>(mindId, out var revRoleComp))
            AddComp(revRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("rev-lieutenant-greeting") }, overwrite: true);
    }

    /// <summary>
    /// (Funky Station) Adds a revolutionary uplink to HRevs. Makes midround HRevs less awkward,
    /// now that they aren't dropping their fucking kit in the middle of security.
    /// </summary>
    /// <returns>true if uplink was successfully added.</returns>
    private bool MakeHeadRevolutionary(EntityUid traitor, RevolutionaryRuleComponent component)
    {
        //Sync Open Revolt state effects to new Head Rev
        if (component.OpenRevoltDeclared && TryComp<HeadRevolutionaryComponent>(traitor, out var headRevComp))
            _revolutionarySystem.ToggleConvertGivesVision((traitor, headRevComp), true);

        //Add Rev Uplink
        if (!_mind.TryGetMind(traitor, out var mindId, out var mind))
            return false;

        var pda = _uplink.FindUplinkTarget(traitor);
        if (pda == null || !_uplink.AddUplink(traitor, component.StartingBalance, component.UplinkCurrencyId, component.UplinkStoreId))
            return false;

        var code = EnsureComp<RingerUplinkComponent>(pda.Value).Code;

        _antag.SendBriefing(traitor, Loc.GetString("head-rev-role-greeting", ("code", string.Join("-", code).Replace("sharp", "#"))), Color.Red, null);

        if (_role.MindHasRole<RevolutionaryRoleComponent>(mindId, out var revRoleComp))
            AddComp(revRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("head-rev-briefing", ("code", string.Join("-", code).Replace("sharp", "#"))) }, overwrite: true);

        return true;
    }

    protected override void ActiveTick(EntityUid uid, RevolutionaryRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.RevLossTimerActive && !component.RevForceLose)
        {
            var headRevList = GetHeadRevs();

            if (!IsGroupDetainedOrDead(headRevList, true, false))
            {
                component.RevLossTimerActive = false;

                for (int i = 0; i < headRevList.Count; i++)
                {
                    _popup.PopupEntity(Loc.GetString("rev-headrev-returned"), headRevList[i], headRevList[i]);
                }
            }
            else if (component.RevLoseTime <= _timing.CurTime)
            {
                component.RevForceLose = true;
                for (int i = 0; i < headRevList.Count; i++)
                {
                    _popup.PopupEntity(Loc.GetString("rev-headrev-abandoned"), headRevList[i], headRevList[i]);
                }
            }
        }

        // funkystation
        if (component.RevVictoryEndTime != null && _timing.CurTime >= component.RevVictoryEndTime)
        {
            EndRound();

            return;
        }

        if (component.CommandCheck <= _timing.CurTime)
        {
            component.CommandCheck = _timing.CurTime + component.TimerWait;

            // goob edit
            if (CheckCommandLose())
            {
                if (!component.HasRevAnnouncementPlayed)
                {
                    _chatSystem.DispatchGlobalAnnouncement(
                        Loc.GetString("revolutionaries-win-announcement"),
                        Loc.GetString("revolutionaries-win-sender"),
                        colorOverride: Color.Gold);

                    component.HasRevAnnouncementPlayed = true;

                    component.RevVictoryEndTime = _timing.CurTime + component.RevVictoryEndDelay;
                }
            }

            if (CheckRevsLose() && !component.HasAnnouncementPlayed)
            {
                DeconvertAllRevs();

                _roundEnd.DoRoundEndBehavior(RoundEndBehavior.ShuttleCall,
                    component.ShuttleCallTime,
                    textCall: "revolutionaries-lose-announcement-shuttle-call",
                    textAnnounce: "revolutionaries-lose-announcement");

                component.HasAnnouncementPlayed = true;
            }

            if (component.OpenRevoltAnnouncementPending)
            {
                //Build string for announcement
                string headRevNameList = "";

                var headRevs = AllEntityQuery<HeadRevolutionaryComponent, MobStateComponent>();
                while (headRevs.MoveNext(out var headRev, out var headRevComp, out _))
                {
                    if (!TryComp<MetaDataComponent>(headRev, out var headRevData))
                        continue;
                    if (headRevNameList.Length > 0)
                        headRevNameList += ", ";
                    headRevNameList += headRevData.EntityName;
                }

                _chatSystem.DispatchGlobalAnnouncement(
                        Loc.GetString("revolutionaries-open-revolt-announcement", ("nameList", headRevNameList)),
                        Loc.GetString("revolutionaries-sender-cc"),
                        colorOverride: Color.Red);
                
                component.OpenRevoltAnnouncementPending = false;
            }
        }
    }

    // funky station
    private void EndRound()
    {
        _roundEnd.EndRound();
    }

    protected override void AppendRoundEndText(EntityUid uid, RevolutionaryRuleComponent component, GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var heads = AllEntityQuery<CommandStaffComponent>();
        var convertedCommand = 0;
        var totalHeadsOfStaff = 0;

        while (heads.MoveNext(out var headUid, out var commandStaffComponent))
        {
            totalHeadsOfStaff += 1;

            if (!commandStaffComponent.Enabled && _mobState.IsAlive(headUid))
                convertedCommand += 1;
        }

        var revsLost = CheckRevsLose();
        var commandLost = CheckCommandLose();
        // This is (revsLost, commandsLost) concatted together
        // (moony wrote this comment idk what it means)
        var index = (commandLost ? 1 : 0) | (revsLost ? 2 : 0);

        // sets index to 4, "rev-total-victory"
        // who needs elegance
        if (convertedCommand.Equals(totalHeadsOfStaff) && !revsLost)
            index = 4;

        args.AddLine(Loc.GetString(Outcomes[index]));

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine(Loc.GetString("rev-headrev-count", ("initialCount", sessionData.Count)));
        foreach (var (mind, data, name) in sessionData)
        {
            _role.MindHasRole<RevolutionaryRoleComponent>(mind, out var role);
            var count = CompOrNull<RevolutionaryRoleComponent>(role)?.ConvertedCount ?? 0;

            args.AddLine(Loc.GetString("rev-headrev-name-user",
                ("name", name),
                ("username", data.UserName),
                ("count", count)));

            // TODO: someone suggested listing all alive? revs maybe implement at some point
        }
    }

    private void OnGetBriefing(EntityUid uid, RevolutionaryRoleComponent comp, ref GetBriefingEvent args)
    {
        var ent = args.Mind.Comp.OwnedEntity;
        var head = HasComp<HeadRevolutionaryComponent>(ent);

        if (!head)
        {
            args.Append(Loc.GetString("rev-briefing"));
        }
    }

    /// <summary>
    /// Called when a Head Rev uses a flash in melee to convert somebody else.
    /// </summary>
    private void OnPostFlash(EntityUid uid, HeadRevolutionaryComponent comp, ref AfterFlashedEvent ev)
    {
        // GoobStation - check if headRev's ability enabled
        if (!comp.ConvertAbilityEnabled)
            return;

        var alwaysConvertible = HasComp<AlwaysRevolutionaryConvertibleComponent>(ev.Target);

        if (!_mind.TryGetMind(ev.Target, out var mindId, out var mind) && !alwaysConvertible)
            return;

        // funkystation - Heretics and Changelings shouldn't be revved
        if (HasComp<ChangelingComponent>(ev.Target) || HasComp<HereticComponent>(ev.Target))
            return;

        // GoobStation - added check if rev is head rev to enable back his convert ability
        if (HasComp<RevolutionaryComponent>(ev.Target) && !HasComp<HeadRevolutionaryComponent>(ev.Target) ||
            HasComp<MindShieldComponent>(ev.Target) ||
            !HasComp<HumanoidAppearanceComponent>(ev.Target) &&
            !alwaysConvertible ||
            !_mobState.IsAlive(ev.Target) ||
            HasComp<ZombieComponent>(ev.Target))
        {
            return;
        }

        if (HasComp<RevolutionEnemyComponent>(ev.Target))
            RemComp<RevolutionEnemyComponent>(ev.Target);

        _npcFaction.AddFaction(ev.Target, RevolutionaryNpcFaction);
        var revComp = EnsureComp<RevolutionaryComponent>(ev.Target);

        if (comp.ConvertGivesRevVision)
            EnsureComp<ShowRevolutionaryIconsComponent>(ev.Target);

        _popup.PopupEntity(Loc.GetString("flash-component-user-head-rev",
            ("victim", Identity.Entity(ev.Target, EntityManager))), ev.Target);

        if (ev.User != null)
        {
            _adminLogManager.Add(LogType.Mind,
                LogImpact.Medium,
                $"{ToPrettyString(ev.User.Value)} converted {ToPrettyString(ev.Target)} into a Revolutionary");

            if (_mind.TryGetMind(ev.User.Value, out var revMindId, out _))
            {
                if (_role.MindHasRole<RevolutionaryRoleComponent>(revMindId, out var ent))
                    ent.Value.Comp2.ConvertedCount++;
            }
        }

        if (mindId == default || !_role.MindHasRole<RevolutionaryRoleComponent>(mindId))
        {
            _role.MindAddRole(mindId, "MindRoleRevolutionary");
        }

        if (mind?.Session != null)
            _antag.SendBriefing(mind.Session, Loc.GetString("rev-role-greeting"), Color.Red, revComp.RevStartSound);

        // Goobstation - Check lose if command was converted
        if (!TryComp<CommandStaffComponent>(ev.Target, out var commandComp))
            return;

        commandComp.Enabled = false;
        CheckCommandLose();
    }

    //~~TODO: Enemies of the revolution~~
    // goob edit: too bad wizden goob did it first :trollface:
    private void OnCommandMobStateChanged(EntityUid uid, CommandStaffComponent comp, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead || ev.NewMobState == MobState.Invalid)
            CheckCommandLose();
    }

    /// <summary>
    /// Checks if all of command is dead and if so will remove all sec and command jobs if there were any left.
    /// </summary>
    private bool CheckCommandLose()
    {
        var commandList = new List<EntityUid>();

        var heads = AllEntityQuery<CommandStaffComponent>();
        while (heads.MoveNext(out var id, out var commandComp)) // GoobStation - commandComp
        {
            // GoobStation - If mindshield was removed from head and he got converted - he won't count as command
            if (commandComp.Enabled)
                commandList.Add(id);
        }

        return IsGroupDetainedOrDead(commandList, true, true);
    }

    private void OnHeadRevMobStateChanged(EntityUid uid, HeadRevolutionaryComponent comp, MobStateChangedEvent ev)
    {
        if (ev.NewMobState == MobState.Dead || ev.NewMobState == MobState.Invalid)
            if (CheckRevsLose())
                DeconvertAllRevs();
    }

    /// <summary>
    /// Funky Station - yeah
    /// </summary>
    private void DeconvertAllRevs()
    {
        var stunTime = TimeSpan.FromSeconds(4);
        var rev = AllEntityQuery<RevolutionaryComponent, MindContainerComponent>();

        while (rev.MoveNext(out var uid, out _, out var mc))
        {
            if (HasComp<HeadRevolutionaryComponent>(uid))
                continue;

            _npcFaction.RemoveFaction(uid, RevolutionaryNpcFaction);
            _stun.TryParalyze(uid, stunTime, true); // todo: use gamerule
            RemCompDeferred<RevolutionaryComponent>(uid);
            RemCompDeferred<ShowRevolutionaryIconsComponent>(uid);
            _popup.PopupEntity(Loc.GetString("rev-break-control", ("name", Identity.Entity(uid, EntityManager))), uid);
            _adminLogManager.Add(LogType.Mind, LogImpact.Medium, $"{ToPrettyString(uid)} was deconverted due to all Head Revolutionaries dying.");

            // Goobstation - check if command staff was deconverted
            if (TryComp<CommandStaffComponent>(uid, out var commandComp))
                commandComp.Enabled = true;

            if (!_mind.TryGetMind(uid, out var mindId, out _, mc))
                continue;

            // remove their antag role
            _role.MindTryRemoveRole<RevolutionaryRoleComponent>(mindId);

            // make it very obvious to the rev they've been deconverted since
            // they may not see the popup due to antag and/or new player tunnel vision
            if (_mind.TryGetSession(mindId, out var session))
                _euiMan.OpenEui(new DeconvertedEui(), session);
        }
    }

    /// <summary>
    /// Checks if all the Head Revs are dead and if so will deconvert all regular revs.
    /// </summary>
    private bool CheckRevsLose() // this should have been just a simple check w no logic
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var revolutionary, out _))
        {
            if (revolutionary.RevForceLose)
                return true;
        }

        var headRevList = GetHeadRevs();

        // If no Head Revs are alive all normal Revs will lose their Rev status and rejoin Nanotrasen
        // Cuffing Head Revs is not enough - they must be killed.
        if (IsGroupDetainedOrDead(headRevList, false, false))
        {
            return true;
        }

        // If Head Revs are all dead OR off station, start the timer
        if (IsGroupDetainedOrDead(headRevList, true, false))
        {
            query = QueryActiveRules();
            while (query.MoveNext(out var uid, out _, out var revolutionary, out _))
            {
                //Do not set this timer again if the last one is still running.
                if (revolutionary.RevLossTimerActive)
                    return false;

                //Start the loss timer, can be reset in ActiveTick if a Head Rev returns to station alive.
                revolutionary.RevLossTimerActive = true;
                revolutionary.RevLoseTime = _timing.CurTime + revolutionary.OffStationTimer;
            }

            for (int i = 0; i < headRevList.Count; i++)
            {
                if (_stationSystem.GetOwningStation(headRevList[i]) == null)
                {
                    _popup.PopupEntity(Loc.GetString("rev-headrev-must-return"), headRevList[i], headRevList[i]); //Popup that the Head Rev must return to the station
                }
            }

            return false;
        }

        return false;
    }

    private List<EntityUid> GetHeadRevs()
    {
        var headRevList = new List<EntityUid>();

        var headRevs = AllEntityQuery<HeadRevolutionaryComponent, MobStateComponent>();
        while (headRevs.MoveNext(out var uid, out var headRevComp, out _)) // GoobStation - headRevComp
        {
            // GoobStation - Checking if headrev ability is enabled to count them
            if (headRevComp.ConvertAbilityEnabled)
                headRevList.Add(uid);
        }

        return headRevList;
    }

    // goob edit - no shuttle call until internal affairs are figured out
    // funkystation - disabled because this is garbo
    private void OnTryCallEvac(ref CommunicationConsoleCallShuttleAttemptEvent ev)
    {
        var revs = EntityQuery<RevolutionaryComponent, MobStateComponent>();
        var revenemies = EntityQuery<RevolutionEnemyComponent, MobStateComponent>();
        var minds = EntityQuery<MindContainerComponent>();

        var revsNormalized = revs.Count() / (minds.Count() - revs.Count());
        var enemiesNormalized = revenemies.Count() / (minds.Count() - revenemies.Count());

        // calling evac will result in an error if:
        // - command is gone & there are more than 35% of enemies
        // - or if there are more than 35% of revolutionaries
        // hardcoded values because idk why not
        // regards
        if (CheckCommandLose() && enemiesNormalized >= .35f
        || revsNormalized >= .35f)
        {
            ev.Cancelled = true;
            ev.Reason = Loc.GetString("shuttle-call-error");
            return;
        }
    }

    // funky station
    public void OnTryShuttleDock(ref ShuttleDockAttemptEvent ev)
    {
        if (!CheckRevsLose())
        {
            ev.Cancelled = true;
            ev.CancelMessage = Loc.GetString("shuttle-dock-fail-revs");
            DeclareOpenRevolt();
        }
    }

    /// <summary>
    /// Will take a group of entities and check if these entities are alive, dead or cuffed.
    /// </summary>
    /// <param name="list">The list of the entities</param>
    /// <param name="checkOffStation">Bool for if you want to check if someone is in space and consider them missing in action. (Won't check when emergency shuttle arrives just in case)</param>
    /// <param name="countCuffed">Bool for if you don't want to count cuffed entities.</param>
    /// <returns></returns>
    private bool IsGroupDetainedOrDead(List<EntityUid> list, bool checkOffStation, bool countCuffed)
    {
        var gone = 0;
        foreach (var entity in list)
        {
            if (TryComp<CuffableComponent>(entity, out var cuffed) && cuffed.CuffedHandCount > 0 && countCuffed)
            {
                gone++;
            }
            else
            {
                if (TryComp<MobStateComponent>(entity, out var state))
                {
                    if (state.CurrentState == MobState.Dead || state.CurrentState == MobState.Invalid)
                    {
                        gone++;
                    }
                    else if (checkOffStation && _stationSystem.IsEntityOnStationGrid(entity) && !_emergencyShuttle.EmergencyShuttleArrived)
                    {
                        gone++;
                    }
                }
                //If they don't have the MobStateComponent they might as well be dead.
                else
                {
                    gone++;
                }
            }
        }

        return gone == list.Count || list.Count == 0;
    }

    /// <summary>
    /// Declares a state of Open Revolt. This allows all Revolutionaries to see each other, at the cost of announcing openly the names of the Head Revolutionaries
    /// </summary>
    private void DeclareOpenRevolt()
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var revolutionaryRule, out _))
        {
            if (revolutionaryRule.OpenRevoltDeclared)
                return;

            revolutionaryRule.OpenRevoltDeclared = true;
            //Queue announcement
            revolutionaryRule.OpenRevoltAnnouncementPending = true;
        }

        var headRevs = AllEntityQuery<HeadRevolutionaryComponent, MobStateComponent>();
        while (headRevs.MoveNext(out var uid, out var headRevComp, out _))
        {
            _revolutionarySystem.ToggleConvertGivesVision((uid, headRevComp), true);
        }

        //Make All Revs see each other's Rev status
        var rev = AllEntityQuery<RevolutionaryComponent, MindContainerComponent>();
        while (rev.MoveNext(out var uid, out _, out var mc))
        {
            EnsureComp<ShowRevolutionaryIconsComponent>(uid);
            _popup.PopupEntity(Loc.GetString("revolutionaries-open-revolt-rev-popup"), uid, uid, PopupType.LargeCaution);
        }
    }

    private void OnHeadRevDeclareOpenRevolt(EntityUid uid, HeadRevolutionaryComponent comp, DeclareOpenRevoltEvent args)
    {
        DeclareOpenRevolt();
        args.Handled = true;
    }

    private static readonly string[] Outcomes =
    {
        // revs survived and heads survived... how
        "rev-reverse-stalemate",
        // revs won and heads died
        "rev-won",
        // revs lost and heads survived
        "rev-lost",
        // revs lost and heads died
        "rev-stalemate",
        // revs won and all heads are converted and healthy
        "rev-total-victory",
    };
}
