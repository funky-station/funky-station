// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Maths;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Content.Shared.GameTicking.Components;
using Content.Server.Roles;
using Content.Server.RoundEnd;
using Content.Server.Antag;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.NPC.Systems;
using Content.Shared.NPC.Prototypes;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Server.BloodCult.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.Administration.Systems;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.Magic.Events;
using Content.Shared.Body.Systems;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Content.Shared.Roles.Jobs;
using Content.Shared.Pinpointer;
using Content.Shared.Actions;
using Content.Shared.Ghost;
using Content.Shared.Database;
using Content.Server.Administration.Logs;
using Content.Shared.Bed.Cryostorage;

using Content.Server.BloodCult.EntitySystems;
using Content.Shared.BloodCult.Prototypes;
using Content.Shared.BloodCult.Prototypes;

using Content.Server.Ghost;
using Content.Server.Ghost.Roles;
using Content.Shared.Ghost.Roles.Raffles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Server.Revolutionary.Components;
using Content.Server.Chat.Systems;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;

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
	[Dependency] private readonly RejuvenateSystem _rejuvenate = default!;
	[Dependency] private readonly GhostRoleSystem _ghostRole = default!;
	[Dependency] private readonly CultistSpellSystem _cultistSpell = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly IGameTiming _timing = default!;
	[Dependency] private readonly ChatSystem _chat = default!;
	[Dependency] private readonly SharedJobSystem _jobs = default!;
	[Dependency] private readonly RoundEndSystem _roundEnd = default!;
	[Dependency] private readonly MobStateSystem _mobSystem = default!;
	[Dependency] private readonly StationSystem _stationSystem = default!;
	[Dependency] private readonly IChatManager _chatManager = default!;
	[Dependency] private readonly ChatSystem _chatSystem = default!;
	[Dependency] private readonly SharedActionsSystem _actions = default!;
	[Dependency] private readonly SharedBodySystem _body = default!;
	[Dependency] private readonly AppearanceSystem _appearance = default!;
	[Dependency] private readonly NpcFactionSystem _npcFaction = default!;
	[Dependency] private readonly IAdminLogManager _adminLogger = default!;

	[Dependency] private readonly IEntityManager _entManager = default!;

	public readonly string CultComponentId = "BloodCultist";

	[ValidatePrototypeId<EntityPrototype>] static EntProtoId mindRole = "MindRoleCultist";

	[ValidatePrototypeId<NpcFactionPrototype>] public readonly ProtoId<NpcFactionPrototype> BloodCultistFactionId = "BloodCultist";
    [ValidatePrototypeId<NpcFactionPrototype>] public readonly ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

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
		component.InitialReportTime = _timing.CurTime + TimeSpan.FromSeconds(1);
		SetConversionsNeeded(component);
		SelectVeilTargets(component);
		component.CheckTime = _timing.CurTime + component.TimerWait;
    }

	private void SelectVeilTargets(BloodCultRuleComponent component)
	{
		var beaconsList = new List<WeakVeilLocation>();

        var beacons = AllEntityQuery<NavMapBeaconComponent, MetaDataComponent>();
        while (beacons.MoveNext(out var beaconUid, out var navMapBeacon, out var metaData))
        {
			if (metaData.EntityPrototype != null &&
				metaData.EntityPrototype.EditorSuffix != null &&
				BloodCultRuleComponent.PossibleVeilLocations.Contains(metaData.EntityPrototype.ID))
			{
				var veilLoc = new WeakVeilLocation(
					metaData.EntityPrototype.EditorSuffix, beaconUid,
					metaData.EntityPrototype.ID, Transform(beaconUid).Coordinates,
					5.0f
				);
				beaconsList.Add(veilLoc);
			}
        }
		if (beaconsList.Count < 3)
			return;
		int first = _random.Next(0, beaconsList.Count);
		int second = _random.Next(0, beaconsList.Count);
		while (second == first)
			second = _random.Next(0, beaconsList.Count);
		int third = _random.Next(0, beaconsList.Count);
		while (third == second || third == first)
			third = _random.Next(0, beaconsList.Count);

		component.WeakVeil1 = beaconsList[first];
		component.WeakVeil2 = beaconsList[second];
		component.WeakVeil3 = beaconsList[third];
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

		// veil has begun to be weakened
		if (component.TargetsDown.Count >= 1)
		{
			if (!component.HasEyes)
			{
				component.HasEyes = true;
				EmpowerCultists(GetCultists());
			}
		}

		// veil has been sufficiently weakened
		if (component.TargetsDown.Count >= component.TargetsRequired)
		{
			if (!component.HasRisen)
			{
				component.HasRisen = true;
				RiseCultists(GetCultists());
			}
			component.Target = null;
			component.VeilWeakened = true;
		}

        // Get all relevant crew
        var allHumans = new HashSet<Entity<MindComponent>>();
		foreach (var person in _mind.GetAliveHumans())//;//(args.MindId);
		{
			var mind = person.Comp;
			var mindCompEntity = mind.Owner;
			if (mindCompEntity != null &&
				_stationSystem.GetOwningStation(mind.CurrentEntity) != null &&
				!HasComp<CryostorageContainedComponent>(mind.CurrentEntity) &&
				!HasComp<CultResistantComponent>(mind.CurrentEntity) &&
				!_role.MindHasRole<BloodCultRoleComponent>(mindCompEntity, out var _))
				allHumans.Add(person);
		}

		// no other humans to kill
		if (allHumans.Count == 0)
        {
			// If there are no remaining possible targets, allow
			//	the cultists to summon Nar'Sie right away.
			if (!component.HasRisen)
			{
				component.HasRisen = true;
				EmpowerCultists(GetCultists());
				RiseCultists(GetCultists());
			}
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
		component.Target = (EntityUid)_random.Pick(allPotentialTargets);

		// Assign original body to track future mis-matches
		var targetMind = CompOrNull<MindComponent>((EntityUid)component.Target);
		if (targetMind != null)
			component.TargetOriginalBody = targetMind.CurrentEntity;
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
				if (component.Target != null && component.Target == mindId)
					SelectTarget(component, true);
			}

			if (TryComp<BloodCultistComponent>(traitor, out var cultist))
			{
				// add cultist starting abilities
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "Commune", recordKnownSpell:false);
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "StudyVeil", recordKnownSpell:false);
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "SpellsSelect", recordKnownSpell:false);

				// propogate the selected Nar'Sie summon location
				cultist.ShowTearVeilRune = component.VeilWeakened;
				cultist.LocationForSummon = component.LocationForSummon;
			}

			if (component.HasEyes)
			{
				if (EntityManager.TryGetComponent(traitor, out AppearanceComponent? appearance))
				{
					_appearance.SetData(traitor, CultEyesVisuals.CultEyes, true, appearance);
				}
			}

			if (component.HasRisen)
			{
				if (EntityManager.TryGetComponent(traitor, out AppearanceComponent? appearance))
				{
					_appearance.SetData(traitor, CultHaloVisuals.CultHalo, true, appearance);
				}
			}

			_npcFaction.RemoveFaction(traitor, NanotrasenFactionId, false);
			_npcFaction.AddFaction(traitor, BloodCultistFactionId);

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

		// Give initial announcement to cultists
		if (component.InitialReportTime != null && _timing.CurTime > component.InitialReportTime)
		{
			AnnounceStatus(component, cultists);
			component.InitialReportTime = null;
		}

		if (component.VeilWeakened && !component.VeilWeakenedAnnouncementPlayed)
		{
			AnnounceStatus(component, cultists);
			component.VeilWeakenedAnnouncementPlayed = true;
			foreach (EntityUid cultist in cultists)
			{
				if (!TryComp<BloodCultistComponent>(cultist, out var cultistComp))
					continue;
				cultistComp.ShowTearVeilRune = true;
				DirtyField(cultist, cultistComp, nameof(BloodCultistComponent.ShowTearVeilRune));
			}
		}

		if (component.Target != null)
		{
			// If the target has gone catatonic, pick another one.
			var targetMind = CompOrNull<MindComponent>((EntityUid)component.Target);
			if (targetMind?.UserId == null)
			{
				SelectTarget(component, true);
				if (!component.VeilWeakened)
				{
					AnnounceStatus(component, cultists);
				}
			}
			// When entered cryo
			if (targetMind != null && HasComp<CryostorageContainedComponent>(targetMind.CurrentEntity))
			{
				SelectTarget(component, true);
				if (!component.VeilWeakened)
				{
					AnnounceStatus(component, cultists);
				}
			}

			// Reselection Timer checks
			if (component.OffStationReselectTimerActive)
			{
				//Re-select target if the allocated time has passed
				if (component.OffStationTargetReselectTime <= _timing.CurTime)
				{
					component.OffStationReselectTimerActive = false;

					SelectTarget(component, true);
					component.MismatchReselectTimerActive = false;
					if (!component.VeilWeakened)
					{
						AnnounceStatus(component, cultists);
					}
				}
				//Target returns to station, stop the timer
				else if (targetMind != null && _stationSystem.GetOwningStation(targetMind.CurrentEntity) != null)
				{
					component.OffStationReselectTimerActive = false;
				}
			}

			if (component.MismatchReselectTimerActive)
			{
				//Re-select target if the allocated time has passed
				if (component.MismatchTargetReselectTime <= _timing.CurTime)
				{
					component.MismatchReselectTimerActive = false;

					SelectTarget(component, true);
					component.OffStationReselectTimerActive = false;
					if (!component.VeilWeakened)
					{
						AnnounceStatus(component, cultists);
					}
				}
				// Target mind returns to original body and is thus identifiable again.
				else if (targetMind != null && targetMind.CurrentEntity == component.TargetOriginalBody)
				{
					component.MismatchReselectTimerActive = false;
				}
			}

			// Check target off station + body mismatch, governs timer starting.
			if (component.CheckTime <= _timing.CurTime)
			{
				component.CheckTime = _timing.CurTime + component.TimerWait;

				if (targetMind != null)
				{
					//If target is off-station, start a 2 minute timer to re-select a target
					if (!component.OffStationReselectTimerActive && _stationSystem.GetOwningStation(targetMind.CurrentEntity) == null)
					{
						component.OffStationTargetReselectTime = _timing.CurTime + component.OffStationTimer;
						component.OffStationReselectTimerActive = true;
					}
					//If target's brain leaves the original body, start a 5 minute timer to re-select a target
					if (!component.MismatchReselectTimerActive && targetMind.CurrentEntity != component.TargetOriginalBody)
					{
						component.MismatchTargetReselectTime = _timing.CurTime + component.MismatchTimer;
						component.MismatchReselectTimerActive = true;
					}
				}
			}
		}

		if (!component.HasEyes && GetConversionsToEyes(component, cultists) == 0)
		{
			component.HasEyes = true;
			EmpowerCultists(cultists);
		}

		if (!component.HasRisen && GetConversionsToRise(component, cultists) == 0)
		{
			component.HasRisen = true;
			RiseCultists(cultists);
		}

		foreach (EntityUid cultistUid in cultists)
		{
			if (!TryComp<BloodCultistComponent>(cultistUid, out var cultist))
				continue;

			// Show cult status
			if (cultist.StudyingVeil)
			{
				AnnounceStatus(component, cultists, cultistUid);
				cultist.StudyingVeil = false;
			}

			// Distribute cult communes
			if (cultist.CommuningMessage != null)
			{
				DistributeCommune(component, cultist.CommuningMessage, cultistUid);
				cultist.CommuningMessage = null;
			}

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
					if (_SacrificeTarget(sacrifice, component, cultistUid))
					{
						AnnounceToCultists(Loc.GetString("cult-narsie-target-down"), newlineNeeded:true);
						component.TotalSacrifices = component.TotalSacrifices + 1;
					}
				}
				else
				{
					// A non-target is being sacrificed!
					if (_SacrificeNonTarget(sacrifice, component, cultistUid))
					{
						AnnounceToCultist(Loc.GetString("cult-narsie-sacrifice-accept"), cultistUid, newlineNeeded:true);
						component.TotalSacrifices = component.TotalSacrifices + 1;
					}
				}

				cultist.Sacrifice = null;
			}

			// Apply active converts
			if (cultist.Convert != null)
			{
				ConvertingData convert = (ConvertingData)cultist.Convert;
				TryComp<MindContainerComponent>(convert.Target, out var convertMind);
				if ((convertMind?.Mind != null) && (component.Target != null) && (convertMind.Mind == component.Target))
				{
					// Override convert and begin sacrificing -- this is a target!
					SacrificingData sacrifice = new SacrificingData(convert.Target, convert.Invokers);
					if (_SacrificeTarget(sacrifice, component, cultistUid))
					{
						AnnounceToCultists(Loc.GetString("cult-narsie-target-down"), newlineNeeded:true);
						component.TotalSacrifices = component.TotalSacrifices + 1;
					}
				}
				else
				{
					// A non-target is being converted.
					_ConvertNonTarget(convert, component, cultistUid);
				}
				cultist.Convert = null;
			}

			// Check for decultification
			if (cultist.DeCultification >= 100.0f)
			{
				RemCompDeferred<BloodCultistComponent>(cultistUid);

				_popupSystem.PopupEntity(Loc.GetString("cult-deconverted"),
					cultistUid, cultistUid, PopupType.LargeCaution
				);

				if (!_mind.TryGetMind(cultistUid, out var mindId, out _))
					continue;

				// remove their antag role
				_role.MindTryRemoveRole<BloodCultRoleComponent>(mindId);

				// reverse their loyalties
				_npcFaction.RemoveFaction(mindId, BloodCultistFactionId, false);
				_npcFaction.AddFaction(mindId, NanotrasenFactionId);

				foreach(var action in _actions.GetActions(cultistUid))
				{
					if (TryComp<CultistSpellComponent>(action.Id, out var actionComp))
						_actions.RemoveAction(cultistUid, action.Id);
				}

				if (EntityManager.TryGetComponent(cultistUid, out AppearanceComponent? appearance))
				{
					_appearance.SetData(cultistUid, CultEyesVisuals.CultEyes, false, appearance);
					_appearance.SetData(cultistUid, CultHaloVisuals.CultHalo, false, appearance);
				}
			}

			// Did someone just try to draw the tear veil rune for the first time?
			if (cultist.TryingDrawTearVeil)
			{
				if (component.WeakVeil1 != null && component.WeakVeil2 != null && component.WeakVeil3 != null)
				{
					WeakVeilLocation? currentSummoningLoc = null;
					WeakVeilLocation weakVeil1 = (WeakVeilLocation)component.WeakVeil1;
					WeakVeilLocation weakVeil2 = (WeakVeilLocation)component.WeakVeil2;
					WeakVeilLocation weakVeil3 = (WeakVeilLocation)component.WeakVeil3;
					if (weakVeil1.Coordinates.InRange(_entManager, Transform(cultistUid).Coordinates, weakVeil1.ValidRadius))
					{
						currentSummoningLoc = weakVeil1;
					}
					else if (weakVeil2.Coordinates.InRange(_entManager, Transform(cultistUid).Coordinates, weakVeil2.ValidRadius))
					{
						currentSummoningLoc = weakVeil2;
					}
					else if (weakVeil3.Coordinates.InRange(_entManager, Transform(cultistUid).Coordinates, weakVeil3.ValidRadius))
					{
						currentSummoningLoc = weakVeil3;
					}

					if (currentSummoningLoc == null)
					{
						// Case : They are not standing in a valid location.
						_popupSystem.PopupEntity(
							Loc.GetString("cult-veil-drawing-toostrong"),
							cultistUid, cultistUid, PopupType.MediumCaution
						);
					}
					else
					{
						//if (cultist.ConfirmedSummonLocation)
						if (!cultist.AskedToConfirm)
						{
							// Case : They are standing in a valid location, but have not been asked to confirm yet.
							string name = ((WeakVeilLocation)currentSummoningLoc).Name;
							_popupSystem.PopupEntity(
								Loc.GetString("cult-veil-drawing-pleaseconfirm", ("name", name)),
								cultistUid, cultistUid, PopupType.MediumCaution
							);
							cultist.AskedToConfirm = true;
						}
						else
						{
							// Case : They are standing in a valid location and have already been asked to confirm. Alert the crew!
							string name = ((WeakVeilLocation)currentSummoningLoc).Name;
							foreach (var currCultist in GetCultists())
							{
								if (!TryComp<BloodCultistComponent>(currCultist, out var cultMember))
									continue;
								cultMember.ConfirmedSummonLocation = true;
								cultMember.LocationForSummon = ((WeakVeilLocation)currentSummoningLoc);
							}
							// Make sure the location for summoning propogates to new cultists.
							component.LocationForSummon = cultist.LocationForSummon;
							_chatSystem.DispatchGlobalAnnouncement(
								Loc.GetString("cult-veil-drawing-crewwarning", ("name", name)),
								"Central Command Higher Dimensional Affairs",
								true,
								new SoundPathSpecifier("/Audio/Announcements/war.ogg"),
								Color.Red);
							//AnnounceToEveryone(Loc.GetString("cult-veil-drawing-crewwarning", ("name", name))+"\n", fontSize:18, newlineNeeded:true);
						}
					}
				}
				cultist.TryingDrawTearVeil = false;
			}

			// Did someone just fail to summon Nar'Sie?
			if (cultist.FailedNarsieSummon)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-invocation-narsie-fail"),
					cultistUid, cultistUid, PopupType.MediumCaution
				);
				cultist.FailedNarsieSummon = false;
			}

			// Summon Nar'Sie
			if (!component.CultistsWin && cultist.NarsieSummoned != null)
			{
				component.CultistsWin = true;
				string newlines = "\n\n\n\n";
				AnnounceToEveryone(newlines+Loc.GetString("cult-veil-torn")+newlines, fontSize:32, audioPath:"/Audio/_Funkystation/Ambience/Antag/dimensional_rend.ogg", audioVolume:2f);
				var narsieSpawn = Spawn("MobNarsieSpawn", (EntityCoordinates)cultist.NarsieSummoned);
				cultist.NarsieSummoned = null;
				component.CultVictoryEndTime = _timing.CurTime + component.CultVictoryEndDelay;
			}
		}

	// End the round
	if (component.CultistsWin && !component.CultVictoryAnnouncementPlayed && component.CultVictoryEndTime != null && _timing.CurTime >= component.CultVictoryEndTime)
	{
		component.CultVictoryAnnouncementPlayed = true;
		component.CultVictoryEndTime = null;

		_chatSystem.DispatchGlobalAnnouncement(
			Loc.GetString("cult-win-announcement"),
			colorOverride: Color.Red);

		EndRound();
		return;
	}
    }

	private void EndRound()
    {
        _roundEnd.EndRound();
    }

	protected override void AppendRoundEndText(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        var sessionData = _antag.GetAntagIdentifiers(uid);
		var cultists = GetCultists();
		if (component.CultistsWin)
			args.AddLine(Loc.GetString("cult-roundend-victory"));
		else
			args.AddLine(Loc.GetString("cult-roundend-failure"));
		args.AddLine(Loc.GetString("cult-roundend-count", ("count", cultists.Count.ToString())));
		args.AddLine(Loc.GetString("cult-roundend-sacrifices", ("sacrifices", component.TotalSacrifices.ToString())));
    }

	private List<EntityUid> GetEveryone(bool includeGhosts = false)
	{
		var everyoneList = new List<EntityUid>();

        var everyone = AllEntityQuery<ActorComponent, MobStateComponent>();
        while (everyone.MoveNext(out var uid, out var actorComp, out _))
        {
            everyoneList.Add(uid);
        }
		if (includeGhosts)
		{
			var ghosts = AllEntityQuery<GhostHearingComponent, ActorComponent>();
			while (ghosts.MoveNext(out var uid, out var _, out var actorComp))
			{
				everyoneList.Add(uid);
			}
		}

        return everyoneList;
	}

	private List<EntityUid> GetCultists(bool includeConstructs = false)
    {
        var cultistList = new List<EntityUid>();

        var cultists = AllEntityQuery<BloodCultistComponent, MobStateComponent>();
		var constructs = AllEntityQuery<BloodCultConstructComponent, MobStateComponent>();
        while (cultists.MoveNext(out var uid, out var cultistComp, out _))
        {
            cultistList.Add(uid);
        }
		if (includeConstructs)
		{
			while (constructs.MoveNext(out var uid, out var constructComp, out _))
			{
				cultistList.Add(uid);
			}
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

		GhostRoleComponent ghostRole = _entManager.AddComponent<GhostRoleComponent>(uid);
		_entManager.AddComponent<GhostTakeoverAvailableComponent>(uid);
		ghostRole.RoleName = Loc.GetString("cult-ghost-role-name");
		ghostRole.RoleDescription = Loc.GetString("cult-ghost-role-desc");
		ghostRole.RoleRules = Loc.GetString("cult-ghost-role-rules");
		ghostRole.RaffleConfig = new GhostRoleRaffleConfig(settings);
		Speak(args.User, Loc.GetString("cult-invocation-revive"));
	}

	public void TrySacrificeVictim(EntityUid uid, BloodCultistComponent comp, ref SacrificeRuneEvent args)
	{
		comp.Sacrifice = new  SacrificingData(args.Target, args.Invokers);
	}

	private bool _SacrificeTarget(SacrificingData sacrifice, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (component.Target == null)
			return false;
		if (sacrifice.Invokers.Length < component.CultistsToSacrificeTarget)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-target-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
		}
		else
		{
			foreach (EntityUid invoker in sacrifice.Invokers)
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			_SacrificeVictim(sacrifice.Target, cultistUid);
			component.ReviveCharges = component.ReviveCharges + component.ChargesForSacrifice;
			component.TargetsDown.Add((EntityUid)component.Target);
			SelectTarget(component, true);
			return true;
		}
	}

	private bool _SacrificeNonTarget(SacrificingData sacrifice, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (HasComp<CultResistantComponent>(sacrifice.Target))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail-resisted"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
		}
		else if (sacrifice.Invokers.Length < component.CultistsToSacrifice)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
		}
		else
		{
			foreach (EntityUid invoker in sacrifice.Invokers)
			{
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			}

			if (_SacrificeVictim(sacrifice.Target, cultistUid))
			{
				component.ReviveCharges = component.ReviveCharges + component.ChargesForSacrifice;
				return true;
			}
		}
		return false;
	}

	private bool _SacrificeVictim(EntityUid uid, EntityUid? casterUid)
	{
		// Remember to use coordinates to play audio if the entity is about to vanish.
		EntityUid? mindId = CompOrNull<MindContainerComponent>(uid)?.Mind;
		MindComponent? mindComp = CompOrNull<MindComponent>(mindId);
		if (mindId != null && mindComp != null)
		{
			var coordinates = Transform(uid).Coordinates;
			_audio.PlayPvs(new SoundPathSpecifier("/Audio/Magic/disintegrate.ogg"), coordinates);
			_body.GibBody(uid, true);
			var soulstone = Spawn("CultSoulStone", coordinates);
			_mind.TransferTo((EntityUid)mindId, soulstone, mind:mindComp);
			return true;
		}
		return false;
	}

	public void TryConvertVictim(EntityUid uid, BloodCultistComponent comp, ref ConvertRuneEvent args)
	{
		comp.Convert = new ConvertingData(args.Target, args.Invokers);
	}

	private bool _ConvertNonTarget(ConvertingData convert, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (HasComp<CultResistantComponent>(convert.Target))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail-resisted"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
		}
		else if (convert.Invokers.Length >= component.CultistsToConvert)
		{
			foreach (EntityUid invoker in convert.Invokers)
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
			component.ReviveCharges = component.ReviveCharges + component.ChargesForSacrifice;
			_ConvertVictim(convert.Target, component);
			return true;
		}
		else
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
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
		_role.MindRemoveRole<BloodCultRoleComponent>(args.Mind.Owner);
	}

	public void Speak(EntityUid? uid, string speech)
	{
		if (uid == null || string.IsNullOrWhiteSpace(speech))
			return;

		var ev = new SpeakSpellEvent((EntityUid)uid, speech);
		RaiseLocalEvent(ref ev);
	}

	public void AnnounceToEveryone(string message, uint fontSize = 14, Color? color = null,
									bool newlineNeeded = false, string? audioPath = null,
									float audioVolume = 1f)
	{
		if (color == null)
			color = Color.DarkRed;
		var filter = Filter.Empty();
		List<EntityUid> everyone = GetEveryone(includeGhosts:true);
		foreach (EntityUid playerUid in everyone)
		{
			if (TryComp(playerUid, out ActorComponent? actorComp))
			{
				filter.AddPlayer(actorComp.PlayerSession);
			}
		}
		string wrappedMessage = "[font size="+fontSize.ToString()+"][bold]" + (newlineNeeded ? "\n" : "") + message + "[/bold][/font]";
		_chatManager.ChatMessageToManyFiltered(filter, ChatChannel.Radio, message, wrappedMessage, default, false, true, color, audioPath, audioVolume);
	}

	public void AnnounceToCultists(string message, uint fontSize = 14, Color? color = null,
									bool newlineNeeded = false, string? audioPath = null,
									float audioVolume = 1f, bool includeGhosts = false)
	{
		if (color == null)
			color = Color.DarkRed;
		var filter = Filter.Empty();
		List<EntityUid> cultists = GetCultists(includeConstructs:true);
		foreach (EntityUid cultistUid in cultists)
		{
			if (TryComp(cultistUid, out ActorComponent? actorComp))
			{
				filter.AddPlayer(actorComp.PlayerSession);
			}
		}
		if (includeGhosts)
		{
			var ghosts = AllEntityQuery<GhostHearingComponent, ActorComponent>();
			while (ghosts.MoveNext(out var uid, out var _, out var actorComp))
			{
				filter.AddPlayer(actorComp.PlayerSession);
			}
		}

		string wrappedMessage = "[font size="+fontSize.ToString()+"][bold]" + (newlineNeeded ? "\n" : "") + message + "[/bold][/font]";
		_chatManager.ChatMessageToManyFiltered(filter, ChatChannel.Radio, message, wrappedMessage, default, false, true, color, audioPath, audioVolume);
		_adminLogger.Add(LogType.Chat, LogImpact.Low, $"Announcement to cultists: {message}");
	}

	public void AnnounceToCultist(string message, EntityUid target, uint fontSize = 14, Color? color = null,
									bool newlineNeeded = false, string? audioPath = null,
									float audioVolume = 1f)
	{
		if (color == null)
			color = Color.DarkRed;
		var filter = Filter.Empty();
		List<EntityUid> cultists = GetCultists(includeConstructs: true);
		foreach (EntityUid cultistUid in cultists)
		{
			if (TryComp(cultistUid, out ActorComponent? actorComp) && cultistUid == target)
			{
				filter.AddPlayer(actorComp.PlayerSession);
			}
		}
		string wrappedMessage = "[font size="+fontSize.ToString()+"][bold]" + (newlineNeeded ? "\n" : "") + message + "[/bold][/font]";
		_chatManager.ChatMessageToManyFiltered(filter, ChatChannel.Radio, message, wrappedMessage, default, false, true, color, audioPath, audioVolume);
	}

	private void SetConversionsNeeded(BloodCultRuleComponent component)
	{
		var allAliveHumans = _mind.GetAliveHumans();
		// 10% cult needed for eyes
		component.ConversionsUntilEyes = (int)Math.Ceiling((float)allAliveHumans.Count * 0.125f);
		// 30% cult needed for rise
		component.ConversionsUntilRise = (int)Math.Ceiling((float)allAliveHumans.Count * 0.3f);
	}

	private int GetConversionsToEyes(BloodCultRuleComponent component, List<EntityUid> cultists)
	{
		// Has the cultist group reached the needed conversions?
		if (component.HasEyes)
			return 0;
		int conversionsUntilEyes = component.ConversionsUntilEyes - cultists.Count;
		conversionsUntilEyes = (conversionsUntilEyes > 0) ? conversionsUntilEyes : 0;
		return conversionsUntilEyes;
	}

	private int GetConversionsToRise(BloodCultRuleComponent component, List<EntityUid> cultists)
	{
		// Has the cultist group reached the needed conversions?
		if (component.HasRisen)
			return 0;
		int conversionsUntilRise = component.ConversionsUntilRise - cultists.Count;
		conversionsUntilRise = (conversionsUntilRise > 0) ? conversionsUntilRise : 0;
		return conversionsUntilRise;
	}

	private void EmpowerCultists(List<EntityUid> cultists)
	{
		// Announce to everyone that the cult is growing stronger and then make eyes glow
		AnnounceToCultists(
			Loc.GetString("cult-ascend-1")+"\n",
			newlineNeeded:true
		);
		foreach (EntityUid cultist in cultists)
		{
			if (EntityManager.TryGetComponent(cultist, out AppearanceComponent? appearance))
			{
				_appearance.SetData(cultist, CultEyesVisuals.CultEyes, true, appearance);
			}
		}
	}

	private void RiseCultists(List<EntityUid> cultists)
	{
		// Announce to everyone that the cult is rising and then do the rising
		AnnounceToCultists(
			Loc.GetString("cult-ascend-2")+"\n",
			newlineNeeded:true
		);
		foreach (EntityUid cultist in cultists)
		{
			if (EntityManager.TryGetComponent(cultist, out AppearanceComponent? appearance))
			{
				_appearance.SetData(cultist, CultHaloVisuals.CultHalo, true, appearance);
			}
		}
	}

	public void AnnounceStatus(BloodCultRuleComponent component, List<EntityUid> cultists, EntityUid? specificCultist = null)
	{
		List<EntityUid> constructs = new List<EntityUid>();
		var constructsQuery = AllEntityQuery<BloodCultConstructComponent, MobStateComponent>();
        while (constructsQuery.MoveNext(out var uid, out var _, out _))
        {
			if (_mobSystem.IsAlive(uid))
            	constructs.Add(uid);
        }
		if (component.CultistsWin)
		{
			if (specificCultist != null)
				AnnounceToCultist("Feed me.\n",
						(EntityUid)specificCultist, color:new Color(111, 80, 143, 255), fontSize:24, newlineNeeded:true);
			else
				AnnounceToCultists("Feed me.\n",
					color:new Color(111, 80, 143, 255), fontSize:24, newlineNeeded:true);
			return;
		}
		string purpleMessage = !component.VeilWeakened ?
				Loc.GetString("cult-status-veil-strong") :
				Loc.GetString("cult-status-veil-weak");

		if (component.VeilWeakened)
		{
			string name1 = "Unknown";
			string name2 = "Unknown";
			string name3 = "Unknown";
			if (component.WeakVeil1 != null)
				name1 = ((WeakVeilLocation)(component.WeakVeil1)).Name;
			if (component.WeakVeil2 != null)
				name2 = ((WeakVeilLocation)(component.WeakVeil2)).Name;
			if (component.WeakVeil3 != null)
				name3 = ((WeakVeilLocation)(component.WeakVeil3)).Name;
			purpleMessage = purpleMessage + "\n" + Loc.GetString("cult-status-veil-weak-goal",
				("firstLoc", name1),
				("secondLoc", name2),
				("thirdLoc", name3));
		}
		else if (component.Target != null)
		{
			//var targ = (Entity<MindComponent>)component.Target;
			if (component.Target != null && TryComp<MindComponent>(component.Target, out var mindComp) && TryComp<MetaDataComponent>(mindComp.OwnedEntity, out var metaData))
			{
				_jobs.MindTryGetJob(component.Target, out var prototype);
				string job = "crewmember.";
				if (prototype != null)
					job = prototype.LocalizedName;
				purpleMessage = purpleMessage + "\n" + Loc.GetString("cult-status-veil-strong-goal", ("targetName", metaData.EntityName),
					("targetJob", job), ("cultistsRequired", component.CultistsToSacrificeTarget.ToString()));
			}
		}
		if (specificCultist != null)
			AnnounceToCultist(purpleMessage,
					(EntityUid)specificCultist, color:new Color(111, 80, 143, 255), fontSize:12, newlineNeeded:true);
		else
			AnnounceToCultists(purpleMessage,
					color:new Color(111, 80, 143, 255), fontSize:12, newlineNeeded:true);

		var conversionsUntilRise = GetConversionsToRise(component, cultists);
		if (specificCultist != null)
			AnnounceToCultist(Loc.GetString("cult-status-veil-weak-cultdata", ("cultCount", (cultists.Count+constructs.Count).ToString()),
				("cultUntilRise", conversionsUntilRise.ToString()), ("cultistCount", cultists.Count.ToString()),
				("constructCount", constructs.Count.ToString())),
				(EntityUid)specificCultist, fontSize: 11, newlineNeeded:true);
		else
			AnnounceToCultists(Loc.GetString("cult-status-veil-weak-cultdata", ("cultCount", (cultists.Count+constructs.Count).ToString()),
				("cultUntilRise", conversionsUntilRise.ToString()), ("cultistCount", cultists.Count.ToString()),
				("constructCount", constructs.Count.ToString())),
				fontSize: 11, newlineNeeded:true);
	}

	public void DistributeCommune(BloodCultRuleComponent component, string message, EntityUid sender)
	{
		string formattedMessage = FormattedMessage.EscapeText(message);

		EntityUid? mindId = CompOrNull<MindContainerComponent>(sender)?.Mind;

		if (mindId != null && TryComp<MetaDataComponent>(sender, out var metaData))
		{
			_chat.TrySendInGameICMessage(sender, Loc.GetString("cult-commune-incantation"), InGameICChatType.Whisper, ChatTransmitRange.Normal);
			_jobs.MindTryGetJob(mindId, out var prototype);
			string job = "Crewmember";
			if (prototype != null)
				job = prototype.LocalizedName;
			AnnounceToCultists(message = Loc.GetString("cult-commune-message", ("name", metaData.EntityName),
				("job", job), ("message", formattedMessage)), color:new Color(166, 27, 27, 255),
				fontSize: 12, newlineNeeded:false, includeGhosts:true);
		}
	}
}
