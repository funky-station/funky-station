using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared.GameTicking.Components;
using Content.Server.Roles;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult;
using Content.Server.BloodCult.Components;
using Content.Shared.Mobs.Components;
using Content.Server.Administration.Systems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Magic.Events;
using Content.Shared.Body.Systems;
using Robust.Shared.Random;
using Content.Server.BloodCult.EntitySystems;
using Content.Shared.BloodCult.Prototypes;
using Content.Shared.BloodCult.Prototypes;

using Content.Server.Ghost;
using Content.Server.Ghost.Roles;
using Content.Shared.Ghost.Roles.Raffles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Server.Revolutionary.Components;

namespace Content.Server.GameTicking.Rules;

/// <summary>
/// Where all the main stuff for Blood Cults happen
/// </summary>
public sealed class BloodCultRuleSystem : GameRuleSystem<BloodCultRuleComponent>
{
	[Dependency] private readonly SharedAudioSystem _audio = default!;
	[Dependency] private readonly AntagSelectionSystem _antag = default!;
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly RoleSystem _role = default!;
	[Dependency] private readonly GhostSystem _ghost = default!;
	[Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
	[Dependency] private readonly GhostRoleSystem _ghostRole = default!;
	[Dependency] private readonly CultistSpellSystem _cultistSpell = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly SharedBodySystem _body = default!;
	[Dependency] private readonly IEntityManager _entManager = default!;

	public readonly string CultComponentId = "BloodCultist";

	[ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleCultist";

	public override void Initialize()
	{
		base.Initialize();
		//SubscribeLocalEvent<CommandStaffComponent, MobStateChangedEvent>(OnCommandMobStateChanged);

		// Do we need a special "head" cultist? Don't think so
        //SubscribeLocalEvent<HeadRevolutionaryComponent, MobStateChangedEvent>(OnHeadRevMobStateChanged);

		SubscribeLocalEvent<BloodCultRuleComponent, AfterAntagEntitySelectedEvent>(AfterEntitySelected); // Funky Station
        SubscribeLocalEvent<BloodCultRoleComponent, GetBriefingEvent>(OnGetBriefing);

		SubscribeLocalEvent<BloodCultistComponent, ReviveRuneAttemptEvent>(TryReviveCultist);
		SubscribeLocalEvent<BloodCultistComponent, GhostifyRuneEvent>(TryGhostifyCultist);
		SubscribeLocalEvent<BloodCultistComponent, SacrificeRuneEvent>(TrySacrificeVictim);
		SubscribeLocalEvent<BloodCultistComponent, ConvertRuneEvent>(TryConvertVictim);

		SubscribeLocalEvent<BloodCultistComponent, MindAddedMessage>(OnMindAdded);
		SubscribeLocalEvent<BloodCultistComponent, MindRemovedMessage>(OnMindRemoved);

		// Do we need a special "head" cultist? Don't think so
		//SubscribeLocalEvent<HeadRevolutionaryComponent, AfterFlashedEvent>(OnPostFlash);
	}

	protected override void Started(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
		SelectTarget(component);
    }

	/// <summary>
    /// Selects a new target for the Cultists. Prioritizes Security and Command.
	/// Cannot select somebody who is already a cultist.
    /// </summary>
	private void SelectTarget(BloodCultRuleComponent component, bool wipe = true)
	{
		// TODO: Should also add a command line command to force a re-selection of the target, so that admins can
		//		intervene if a target is like way out in the middle of nowhere or something.

		// target already assigned
        if (!wipe && component.Target != null)
            return;

        // no other humans to kill
        var allHumans = new HashSet<Entity<MindComponent>>();
		foreach (var person in _mind.GetAliveHumans())//;//(args.MindId);
		{
			if (TryComp<MindComponent>(person, out var mind) &&
				!_role.MindHasRole<BloodCultRoleComponent>(person, out var _))
				allHumans.Add(person);
		}

		if (allHumans.Count == 0)
        {
			// If there are no remaining possible targets, allow
			//	the cultists to summon Nar'Sie right away.
			component.Target = null;
			component.VeilWeakened = true;
            return;
        }

		// try to pick sec/command
        var allPotentialTargets = new HashSet<Entity<MindComponent>>();
        foreach (var person in allHumans)
        {
            if (TryComp<MindComponent>(person, out var mind) &&
				mind.OwnedEntity is { } ent &&
				(HasComp<CommandStaffComponent>(ent) || HasComp<SecurityStaffComponent>(ent)))
                allPotentialTargets.Add(person);
        }

		// if there are no sec/command, just pick from the general crew pool
        if (allPotentialTargets.Count == 0)
            allPotentialTargets = allHumans; // fallback to non-head and non-sec target

		// TODO: Check for TimeOfDeath being null (makes sure they're not dead)
		// Can also check to see if Session is null, which means they logged out
		Console.WriteLine("Possible targets:");
		foreach (var target in allPotentialTargets)
		{
			Console.WriteLine(target.Comp.CharacterName);
		}
		component.Target = _random.Pick(allPotentialTargets);
		Console.WriteLine("TARGET IS");
		Console.WriteLine(component.Target);
	}

	private void AfterEntitySelected(Entity<BloodCultRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        MakeCultist(args.EntityUid, ent);
    }

	/// <summary>
    /// Supplies new cultists with what they need.
    /// </summary>
    /// <returns>true if cultist was successfully added.</returns>
    private bool MakeCultist(EntityUid traitor, BloodCultRuleComponent component)
    {
        if (_TryAssignCultMind(traitor))
		{
			// re-select target if needed
			if (_mind.TryGetMind(traitor, out var mindId, out var _))
			{
				if (component.Target == mindId)
					SelectTarget(component, true);
			}

			if (TryComp<BloodCultistComponent>(traitor, out var cultist))
			{
				// add cultist dagger spell
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "SummonDagger");//rit.OutputKnowledge);
				
			}
			return true;
		}
		return false;
	}

	private bool _TryAssignCultMind(EntityUid traitor)
	{
		if (!_mind.TryGetMind(traitor, out var mindId, out var mind))
            return false;

		_role.MindAddRole(mindId, mindRole.Id, mind, true);

		EnsureComp<BloodCultistComponent>(traitor);

        _antag.SendBriefing(traitor, Loc.GetString("cult-role-greeting"), Color.Red, null);

        if (_role.MindHasRole<BloodCultRoleComponent>(mindId, out var cultRoleComp))
			AddComp(cultRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("cult-briefing") }, overwrite: true);
            //AddComp(cultRoleComp.Value, new RoleBriefingComponent { Briefing = Loc.GetString("head-rev-briefing", ("code", string.Join("-", code).Replace("sharp", "#"))) }, overwrite: true);

        return true;
	}

	protected override void ActiveTick(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

		List<EntityUid> cultists = GetCultists();
		foreach (EntityUid cultistUid in cultists)
		{
			if (!TryComp<BloodCultistComponent>(cultistUid, out var cultist))
				continue;

			// Apply active revives
			if (cultist.BeingRevived)
			{
				if (component.ReviveCharges >= component.CostToRevive)
				{
					component.ReviveCharges = component.ReviveCharges - component.CostToRevive;
					_ReviveCultist(cultistUid, cultist.ReviverUid);
				}
				else if (cultist.ReviverUid != null)
				{
					_popupSystem.PopupEntity(
							Loc.GetString("cult-invocation-revive-fail"),
							(EntityUid)cultist.ReviverUid, (EntityUid)cultist.ReviverUid, PopupType.MediumCaution
						);
				}
				cultist.BeingRevived = false;
				cultist.ReviverUid = null;
			}

			// Apply active sacrifices
			if (cultist.Sacrifice != null)
			{
				SacrificingData sacrifice = (SacrificingData)cultist.Sacrifice;
				TryComp<MindContainerComponent>(sacrifice.Target, out var sacrificeMind);

				if ((sacrificeMind?.Mind != null) && (component.Target != null) && (sacrificeMind.Mind == component.Target))
				{
					// A target is being sacrificed!
					_SacrificeTarget(sacrifice, component, cultistUid);
				}
				else
				{
					// A non-target is being sacrificed!
					_SacrificeNonTarget(sacrifice, component, cultistUid);
				}

				cultist.Sacrifice = null;
			}

			// Apply active converts
			if (cultist.Convert != null)
			{
				Console.WriteLine("ACTIVE CONVERT HAPPENING");
				ConvertingData convert = (ConvertingData)cultist.Convert;
				TryComp<MindContainerComponent>(convert.Target, out var convertMind);
				if ((convertMind?.Mind != null) && (component.Target != null) && (convertMind.Mind == component.Target))
				{
					// Override convert and begin sacrificing -- this is a target!
					SacrificingData sacrifice = new SacrificingData(convert.Target, convert.Invokers);
					_SacrificeTarget(sacrifice, component, cultistUid);
				}
				else
				{
					// A non-target is being converted.
					_ConvertNonTarget(convert, component, cultistUid);
				}
				cultist.Convert = null;
			}
		}

		// Step 1: Check to see if the Cultists have lost.
        //if (component.RevLossTimerActive && !component.RevForceLose)
        //{
        //    var headRevList = GetHeadRevs();

        //    if (!IsGroupDetainedOrDead(headRevList, true, false))
        //    {
        //        component.RevLossTimerActive = false;

        //        for (int i = 0; i < headRevList.Count; i++)
        //        {
        //            _popup.PopupEntity(Loc.GetString("rev-headrev-returned"), headRevList[i], headRevList[i]);
        //        }
        //    }
        //    else if (component.RevLoseTime <= _timing.CurTime)
        //    {
        //        component.RevForceLose = true;
        //        for (int i = 0; i < headRevList.Count; i++)
        //        {
        //            _popup.PopupEntity(Loc.GetString("rev-headrev-abandoned"), headRevList[i], headRevList[i]);
        //        }
        //    }
        //}

		// Step 2: Check to see if the Cultists have timed out. (?)
        // funkystation
        //if (component.RevVictoryEndTime != null && _timing.CurTime >= component.RevVictoryEndTime)
        //{
        //    EndRound();

        //    return;
        //}

		// Step 3: Check to see if the Cultists have summoned Nar'Sie.
        //if (component.CommandCheck <= _timing.CurTime)
        //{
        //    component.CommandCheck = _timing.CurTime + component.TimerWait;

        //    // goob edit
        //    if (CheckCommandLose())
        //    {
        //        if (!component.HasRevAnnouncementPlayed)
        //        {
        //            _chatSystem.DispatchGlobalAnnouncement(
        //                Loc.GetString("revolutionaries-win-announcement"),
        //                Loc.GetString("revolutionaries-win-sender"),
        //                colorOverride: Color.Gold);

        //            component.HasRevAnnouncementPlayed = true;

        //            component.RevVictoryEndTime = _timing.CurTime + component.RevVictoryEndDelay;
        //        }
        //    }

        //    if (CheckRevsLose() && !component.HasAnnouncementPlayed)
        //    {
        //        DeconvertAllRevs();

        //        _roundEnd.DoRoundEndBehavior(RoundEndBehavior.ShuttleCall,
        //            component.ShuttleCallTime,
        //            textCall: "revolutionaries-lose-announcement-shuttle-call",
        //            textAnnounce: "revolutionaries-lose-announcement");

        //        component.HasAnnouncementPlayed = true;
        //    }
        //}
    }

	protected override void AppendRoundEndText(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

		// Which team won?
        //var revsLost = CheckRevsLose();
        //var commandLost = CheckCommandLose();
        
		// This is (revsLost, commandsLost) concatted together
        // (moony wrote this comment idk what it means)
        //var index = (commandLost ? 1 : 0) | (revsLost ? 2 : 0);
        args.AddLine("The super duper cult round is over!");//Loc.GetString(Outcomes[index]));

        var sessionData = _antag.GetAntagIdentifiers(uid);
        args.AddLine("This is where the number of cultists will go!");//Loc.GetString("rev-headrev-count", ("initialCount", sessionData.Count)));

		//foreach (var (mind, data, name) in sessionData)
        //{
        //    _role.MindHasRole<BloodCultRoleComponent>(mind, out var role);
        //    var count = CompOrNull<BloodCultRoleComponent>(role)?.ConvertedCount ?? 0;

        //    args.AddLine(Loc.GetString("rev-headrev-name-user",
        //        ("name", name),
        //        ("username", data.UserName),
        //        ("count", count)));

            // TODO: someone suggested listing all alive? revs maybe implement at some point
        //}
    }

	private List<EntityUid> GetCultists()
    {
        var cultistList = new List<EntityUid>();

        var cultists = AllEntityQuery<BloodCultistComponent, MobStateComponent>();
        while (cultists.MoveNext(out var uid, out var cultistComp, out _)) // GoobStation - headRevComp
        {
            cultistList.Add(uid);
        }

        return cultistList;
    }

	private void OnGetBriefing(EntityUid uid, BloodCultRoleComponent comp, ref GetBriefingEvent args)
    {
		args.Append(Loc.GetString("cult-briefing-targets"));
    }

	public void TryReviveCultist(EntityUid uid, BloodCultistComponent comp, ref ReviveRuneAttemptEvent args)
	{
		comp.ReviverUid = args.User;
		comp.BeingRevived = true;
	}

	private void _ReviveCultist(EntityUid uid, EntityUid? casterUid)
	{
		Speak(casterUid, Loc.GetString("cult-invocation-revive"));
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/staff_healing.ogg"), uid);
		_rejuvenate.PerformRejuvenate(uid);
	}

	private void TryGhostifyCultist(EntityUid uid, BloodCultistComponent comp, ref GhostifyRuneEvent args)
	{
		if (HasComp<GhostRoleComponent>(uid) || HasComp<GhostTakeoverAvailableComponent>(uid))
			return;

		/*
		var settings = new GhostRoleRaffleSettings()
		{
			InitialDuration = initial,
			JoinExtendsDurationBy = extends,
			MaxDuration = max
		};
		var ghostRoleInfo = new GhostRoleInfo//()
		{
			Identifier = id,
			Name = role.RoleName,
			Description = role.RoleDescription,
			Rules = role.RoleRules,
			Requirements = role.Requirements,
			Kind = kind,
			RafflePlayerCount = rafflePlayerCount,
			RaffleEndTime = raffleEndTime
		};
		*/

		GhostRoleRaffleSettings settings;
		settings = new GhostRoleRaffleSettings()
		{
			InitialDuration = 20,
			JoinExtendsDurationBy = 5,
			MaxDuration = 30
		};

		GhostRoleComponent ghostRole = _entManager.AddComponent<GhostRoleComponent>(uid);//(uid.Value);
		_entManager.AddComponent<GhostTakeoverAvailableComponent>(uid);//(uid.Value);
		ghostRole.RoleName = Loc.GetString("cult-ghost-role-name");//name;
		ghostRole.RoleDescription = Loc.GetString("cult-ghost-role-desc");//description;
		ghostRole.RoleRules = Loc.GetString("cult-ghost-role-rules");//rules;
		ghostRole.RaffleConfig = new GhostRoleRaffleConfig(settings);
		Speak(args.User, Loc.GetString("cult-invocation-revive"));
	}

	public void TrySacrificeVictim(EntityUid uid, BloodCultistComponent comp, ref SacrificeRuneEvent args)
	{
		comp.Sacrifice = new  SacrificingData(args.Target, args.Invokers);
	}

	private void _SacrificeTarget(SacrificingData sacrifice, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (component.Target == null)
			return;
		if (sacrifice.Invokers.Length < component.CultistsToSacrificeTarget)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-target-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
		}
		else
		{
			foreach (EntityUid invoker in sacrifice.Invokers)
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			_SacrificeVictim(sacrifice.Target, cultistUid);
			component.ReviveCharges = component.ReviveCharges + component.ChargesForSacrifice;
			component.TargetsDown.Add((EntityUid)component.Target);
			SelectTarget(component, true);
		}
	}

	private void _SacrificeNonTarget(SacrificingData sacrifice, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (sacrifice.Invokers.Length < component.CultistsToSacrifice)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
		}
		else
		{
			foreach (EntityUid invoker in sacrifice.Invokers)
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			_SacrificeVictim(sacrifice.Target, cultistUid);
			component.ReviveCharges = component.ReviveCharges + component.ChargesForSacrifice;
		}
	}

	private void _SacrificeVictim(EntityUid uid, EntityUid? casterUid)
	{
		// Remember to use coordinates to play audio if the entity is about to vanish.
		EntityUid? mindId = CompOrNull<MindContainerComponent>(uid)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		if (mindId != null)
		{
			_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), Transform(uid).Coordinates);
			_body.GibBody(uid, true);
			_ghost.OnGhostAttempt((EntityUid)mindId, false, false, mindComp);
			// TODO: Spawn Soulstone Shard with their consciousness, making
			// that their new brain
		}
	}

	public void TryConvertVictim(EntityUid uid, BloodCultistComponent comp, ref ConvertRuneEvent args)
	{
		comp.Convert = new ConvertingData(args.Target, args.Invokers);
	}

	private void _ConvertNonTarget(ConvertingData convert, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (convert.Invokers.Length >= component.CultistsToConvert)
		{
			foreach (EntityUid invoker in convert.Invokers)
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			_ConvertVictim(convert.Target, component);
		}
		else
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
		}
	}

	private void _ConvertVictim(EntityUid uid, BloodCultRuleComponent component)
	{
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/_Funkystation/Ambience/Antag/creepyshriek.ogg"), uid);
		MakeCultist(uid, component);
		_rejuvenate.PerformRejuvenate(uid);
	}

	private void OnMindAdded(EntityUid uid, BloodCultistComponent cultist, MindAddedMessage args)
	{
		_TryAssignCultMind(uid);
	}

	private void OnMindRemoved(EntityUid uid, BloodCultistComponent cultist, MindRemovedMessage args)
	{
		_role.MindRemoveRole<BloodCultRoleComponent>(args.Mind);
	}

	public void Speak(EntityUid? uid, string speech)//(BaseActionEvent args)
	{
		if (uid == null || string.IsNullOrWhiteSpace(speech))
			return;

		var ev = new SpeakSpellEvent((EntityUid)uid, speech);//speak.Speech);
		RaiseLocalEvent(ref ev);
	}
}
