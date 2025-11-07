// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
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

using Content.Server.Ghost;
using Content.Server.Ghost.Roles;
using Content.Shared.Ghost.Roles.Raffles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Ghost.Roles.Raffles;
using Content.Server.Revolutionary.Components;
using Content.Server.Chat.Systems;
using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Robust.Shared.Console;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.Speech;
using Content.Shared.Emoting;

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
	[Dependency] private readonly CultistSpellSystem _cultistSpell = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly IGameTiming _timing = default!;
	[Dependency] private readonly ChatSystem _chat = default!;
	[Dependency] private readonly SharedPhysicsSystem _physics = default!;
	[Dependency] private readonly SharedJobSystem _jobs = default!;
	[Dependency] private readonly RoundEndSystem _roundEnd = default!;
	[Dependency] private readonly MobStateSystem _mobSystem = default!;
	[Dependency] private readonly IChatManager _chatManager = default!;
	[Dependency] private readonly SharedActionsSystem _actions = default!;
	[Dependency] private readonly SharedBodySystem _body = default!;
	[Dependency] private readonly AppearanceSystem _appearance = default!;
	[Dependency] private readonly NpcFactionSystem _npcFaction = default!;
	[Dependency] private readonly IAdminLogManager _adminLogger = default!;
	[Dependency] private readonly IConsoleHost _consoleHost = default!;
	[Dependency] private readonly SharedTransformSystem _transformSystem = default!;

	public readonly string CultComponentId = "BloodCultist";

	private static readonly EntProtoId MindRole = "MindRoleCultist";

	public static readonly ProtoId<NpcFactionPrototype> BloodCultistFactionId = "BloodCultist";
    public static readonly ProtoId<NpcFactionPrototype> NanotrasenFactionId = "NanoTrasen";

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

		// Register admin commands
		InitializeCommands();
	}

	private void InitializeCommands()
	{
		_consoleHost.RegisterCommand("cult_queryblood",
			"Query the current blood collected and remaining for the Blood Cult game rule",
			"cult_queryblood",
			QueryBloodCommand);

		_consoleHost.RegisterCommand("cult_setblood",
			"Set the current blood amount for the Blood Cult game rule",
			"cult_setblood <amount>",
			SetBloodCommand);
	}

	protected override void Started(EntityUid uid, BloodCultRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);
		// Calculate blood requirements based on player count instead of selecting targets
		CalculateBloodRequirements(component);
		component.InitialReportTime = _timing.CurTime + TimeSpan.FromSeconds(1);
		SetConversionsNeeded(component);
		SetMinimumCultistsForVeilRitual(component);
		SelectVeilTargets(component);
    }

	/// <summary>
	/// Calculates blood requirements for each phase based on current player count.
	/// Each phase requires 100u of blood per player.
	/// </summary>
	private void CalculateBloodRequirements(BloodCultRuleComponent component)
	{
		var allAliveHumans = _mind.GetAliveHumans();
		double bloodPerPlayer = 100.0;
		
		// Calculate blood needed for each phase
		double totalBloodForPhase = allAliveHumans.Count * bloodPerPlayer;
		
		component.BloodRequiredForEyes = totalBloodForPhase;
		component.BloodRequiredForRise = totalBloodForPhase * 2.0; // Second phase requires cumulative blood
		component.BloodRequiredForVeil = totalBloodForPhase * 3.0; // Third phase requires cumulative blood
		
		// Reset blood collected to 0
		component.BloodCollected = 0.0;
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
			if (TryComp<BloodCultistComponent>(traitor, out var cultist))
			{
				// add cultist starting abilities
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "Commune", recordKnownSpell:false);
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "StudyVeil", recordKnownSpell:false);
				_cultistSpell.AddSpell(traitor, cultist, (ProtoId<CultAbilityPrototype>) "SpellsSelect", recordKnownSpell:false);

				// propogate the selected Nar'Sie summon location
				// Enable Tear Veil rune if stage 2 (HasRisen) or later has been reached
				cultist.ShowTearVeilRune = component.HasRisen || component.VeilWeakened;
				cultist.LocationForSummon = component.LocationForSummon;
			}

			if (component.HasEyes)
			{
				if (EntityManager.TryGetComponent(traitor, out AppearanceComponent? appearance))
				{
					_appearance.SetData(traitor, CultEyesVisuals.CultEyes, true, appearance);
				}
			}

			if (component.VeilWeakened)
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

		_role.MindAddRole(mindId, MindRole, mind, true);

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

		// Check if blood thresholds have been reached for stage progression
		if (!component.HasEyes && component.BloodCollected >= component.BloodRequiredForEyes)
		{
			component.HasEyes = true;
			EmpowerCultists(cultists);
			AnnounceStatus(component, cultists);
		}

		if (!component.HasRisen && component.BloodCollected >= component.BloodRequiredForRise)
		{
			component.HasRisen = true;
			// RiseCultists moved to CompleteVeilRitual (when VeilWeakened becomes true)
			AnnounceStatus(component, cultists);
			
			// Enable Tear Veil rune for all cultists at stage 2
			foreach (EntityUid cultist in cultists)
			{
				if (!TryComp<BloodCultistComponent>(cultist, out var cultistComp))
					continue;
				cultistComp.ShowTearVeilRune = true;
				DirtyField(cultist, cultistComp, nameof(BloodCultistComponent.ShowTearVeilRune));
			}
		}

		// Stage 3 (VeilWeakened) requires the Tear the Veil ritual to be completed
		// This is handled by the TearVeilSystem and cannot be triggered by blood collection alone

		// Disabled: Conversion-based progression conflicts with blood-based progression
		// The blood system provides better gameplay and doesn't trigger prematurely with low player counts
		// if (!component.HasEyes && GetConversionsToEyes(component, cultists) == 0)
		// {
		// 	component.HasEyes = true;
		// 	EmpowerCultists(cultists);
		// }
		//
		// if (!component.HasRisen && GetConversionsToRise(component, cultists) == 0)
		// {
		// 	component.HasRisen = true;
		// 	RiseCultists(cultists);
		// }

		foreach (EntityUid cultistUid in cultists)
		{
			if (!TryComp<BloodCultistComponent>(cultistUid, out var cultist))
				continue;

			// Ensure ShowTearVeilRune is always correct based on current stage
			bool shouldShowTearVeil = component.HasRisen || component.VeilWeakened;
			if (cultist.ShowTearVeilRune != shouldShowTearVeil)
			{
				cultist.ShowTearVeilRune = shouldShowTearVeil;
				DirtyField(cultistUid, cultist, nameof(BloodCultistComponent.ShowTearVeilRune));
			}

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
				_ReviveCultist(cultistUid, cultist.ReviverUid);
				cultist.BeingRevived = false;
				cultist.ReviverUid = null;
			}

			// Apply active sacrifices
			if (cultist.Sacrifice != null)
			{
				SacrificingData sacrifice = (SacrificingData)cultist.Sacrifice;
				
				if (_SacrificeOffering(sacrifice, component, cultistUid))
				{
					AnnounceToCultist(Loc.GetString("cult-narsie-sacrifice-accept"), cultistUid, newlineNeeded:true);
					component.TotalSacrifices = component.TotalSacrifices + 1;
				}

				cultist.Sacrifice = null;
			}

			// Apply active converts
			if (cultist.Convert != null)
			{
				ConvertingData convert = (ConvertingData)cultist.Convert;
				_ConvertOffering(convert, component, cultistUid);
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

			//EndRound();
			_roundEnd.DoRoundEndBehavior(RoundEndBehavior.ShuttleCall,
				component.ShuttleCallTime,
				textCall: "cult-win-announcement-shuttle-call",
				textAnnounce: "cult-win-announcement");
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

		GhostRoleComponent ghostRole = AddComp<GhostRoleComponent>(uid);
		EnsureComp<GhostTakeoverAvailableComponent>(uid);
		ghostRole.RoleName = Loc.GetString("cult-ghost-role-name");
		ghostRole.RoleDescription = Loc.GetString("cult-ghost-role-desc");
		ghostRole.RoleRules = Loc.GetString("cult-ghost-role-rules");
		ghostRole.RaffleConfig = new GhostRoleRaffleConfig(settings);
		Speak(args.User, Loc.GetString("cult-invocation-revive"));
	}

	public void TrySacrificeVictim(EntityUid uid, BloodCultistComponent comp, ref SacrificeRuneEvent args)
	{
		comp.Sacrifice = new  SacrificingData(args.Victim, args.Invokers);
	}

	private bool _SacrificeOffering(SacrificingData sacrifice, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (HasComp<CultResistantComponent>(sacrifice.Victim))
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
		// Only make the minimum required number of cultists speak
		int speakerCount = 0;
		foreach (EntityUid invoker in sacrifice.Invokers)
		{
			if (speakerCount >= component.CultistsToSacrifice)
				break;
			
			Speak(invoker, Loc.GetString("cult-invocation-offering"));
			speakerCount++;
		}

		if (_SacrificeVictim(sacrifice.Victim, cultistUid))
		{
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
			
			// Ensure the soulstone can speak but not move
			EnsureComp<SpeechComponent>(soulstone);
			EnsureComp<EmotingComponent>(soulstone);
		
		// Give the soulstone a physics push for visual effect
		if (TryComp<PhysicsComponent>(soulstone, out var physics))
		{
			// Wake the physics body so it responds to the impulse
			_physics.SetAwake((soulstone, physics), true);
			
			// Generate a random direction and speed (5-10 units/sec similar to a weak throw)
			var randomDirection = _random.NextVector2();
			var speed = _random.NextFloat(5f, 10f);
			var impulse = randomDirection * speed * physics.Mass;
			_physics.ApplyLinearImpulse(soulstone, impulse, body: physics);
		}
			
			return true;
		}
		return false;
	}

	public void TryConvertVictim(EntityUid uid, BloodCultistComponent comp, ref ConvertRuneEvent args)
	{
		comp.Convert = new ConvertingData(args.Subject, args.Invokers);
	}

	private bool _ConvertOffering(ConvertingData convert, BloodCultRuleComponent component, EntityUid cultistUid)
	{
		if (HasComp<CultResistantComponent>(convert.Subject))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-invocation-fail-resisted"),
				cultistUid, cultistUid, PopupType.MediumCaution
			);
			return false;
		}
		else if (convert.Invokers.Length >= component.CultistsToConvert)
		{
			// Only make the minimum required number of cultists speak
			int speakerCount = 0;
			foreach (EntityUid invoker in convert.Invokers)
			{
				if (speakerCount >= component.CultistsToConvert)
					break;
				
				Speak(invoker, Loc.GetString("cult-invocation-offering"));
				speakerCount++;
			}
			
			_ConvertVictim(convert.Subject, component);
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

	/// <summary>
	/// Generates a random cult chant by combining phrases from the cult-chants.ftl localization file.
	/// </summary>
	/// <param name="wordCount">Number of words in the chant (default: 2)</param>
	/// <returns>A randomly generated cult chant</returns>
	public string GenerateChant(int wordCount = 2)
	{
		const int totalChants = 15; // Total number of cult-chant-X entries in cult-chants.ftl
		
		if (wordCount < 1)
			wordCount = 1;
		
		var chantParts = new List<string>();
		for (int i = 0; i < wordCount; i++)
		{
			var chantIndex = Random.Shared.Next(1, totalChants + 1);
			chantParts.Add(Loc.GetString($"cult-chant-{chantIndex}"));
		}
		
		return string.Join(" ", chantParts);
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

	/// <summary>
	/// Calculates the minimum number of cultists required for the Tear Veil ritual based on player count.
	/// Uses 1/8th of total players (12.5%), with a minimum of 2 cultists.
	/// </summary>
	private void SetMinimumCultistsForVeilRitual(BloodCultRuleComponent component)
	{
		var allAliveHumans = _mind.GetAliveHumans();
		// 1/8th (12.5%) of players, minimum of 2
		component.MinimumCultistsForVeilRitual = Math.Max(2, (int)Math.Ceiling((float)allAliveHumans.Count * 0.125f));
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
		if (specificCultist != null)
			AnnounceToCultist(purpleMessage,
					(EntityUid)specificCultist, color:new Color(111, 80, 143, 255), fontSize:12, newlineNeeded:true);
		else
			AnnounceToCultists(purpleMessage,
					color:new Color(111, 80, 143, 255), fontSize:12, newlineNeeded:true);

	if (specificCultist != null)
		AnnounceToCultist(Loc.GetString("cult-status-cultdata", ("cultCount", (cultists.Count+constructs.Count).ToString()),
			("cultistCount", cultists.Count.ToString()),
			("constructCount", constructs.Count.ToString())),
			(EntityUid)specificCultist, fontSize: 11, newlineNeeded:true);
	else
		AnnounceToCultists(Loc.GetString("cult-status-cultdata", ("cultCount", (cultists.Count+constructs.Count).ToString()),
			("cultistCount", cultists.Count.ToString()),
			("constructCount", constructs.Count.ToString())),
			fontSize: 11, newlineNeeded:true);

	// Display blood collection progress
	string bloodMessage = GetBloodProgressMessage(component);
	if (specificCultist != null)
		AnnounceToCultist(bloodMessage, (EntityUid)specificCultist, color: new Color(139, 0, 0, 255), fontSize: 11, newlineNeeded: true);
	else
		AnnounceToCultists(bloodMessage, color: new Color(139, 0, 0, 255), fontSize: 11, newlineNeeded: true);
	}

	private string GetBloodProgressMessage(BloodCultRuleComponent component)
	{
		double currentBlood = component.BloodCollected;
		string currentPhase = "";
		double nextThreshold = 0.0;
		double bloodNeeded = 0.0;

		// Determine current phase and next threshold
		if (!component.HasEyes)
		{
			currentPhase = "Eyes";
			nextThreshold = component.BloodRequiredForEyes;
			bloodNeeded = nextThreshold - currentBlood;
		}
		else if (!component.HasRisen)
		{
			currentPhase = "Rise";
			nextThreshold = component.BloodRequiredForRise;
			bloodNeeded = nextThreshold - currentBlood;
		}
		else if (!component.VeilWeakened)
		{
			// Stage 2 complete - need to do Tear Veil ritual
			nextThreshold = component.BloodRequiredForRise;
			
			string message = Loc.GetString("cult-blood-progress-stage-complete",
				("bloodCollected", Math.Round(currentBlood, 1).ToString()),
				("totalRequired", Math.Round(nextThreshold, 1).ToString()));
			
			// Show Tear Veil locations if they exist
			if (component.WeakVeil1 != null && component.WeakVeil2 != null && component.WeakVeil3 != null)
			{
				string name1 = ((WeakVeilLocation)component.WeakVeil1).Name;
				string name2 = ((WeakVeilLocation)component.WeakVeil2).Name;
				string name3 = ((WeakVeilLocation)component.WeakVeil3).Name;
				message += "\n" + Loc.GetString("cult-blood-progress-tear-veil",
					("location1", name1),
					("location2", name2),
					("location3", name3));
			}
			
			return message;
		}
		else
		{
			// Stage 3 - Veil is weakened, need to do final summoning
			nextThreshold = component.BloodRequiredForVeil;
			
			string message = Loc.GetString("cult-blood-progress-stage-complete",
				("bloodCollected", Math.Round(currentBlood, 1).ToString()),
				("totalRequired", Math.Round(nextThreshold, 1).ToString()));
			
			message += "\n" + Loc.GetString("cult-blood-progress-final-summon");
			
			return message;
		}

		return Loc.GetString("cult-blood-progress",
			("bloodCollected", Math.Round(currentBlood, 1).ToString()),
			("bloodNeeded", Math.Round(bloodNeeded, 1).ToString()),
			("nextPhase", currentPhase),
			("totalRequired", Math.Round(nextThreshold, 1).ToString()));
	}

	public void DistributeCommune(BloodCultRuleComponent component, string message, EntityUid sender)
	{
		string formattedMessage = FormattedMessage.EscapeText(message);

		EntityUid? mindId = CompOrNull<MindContainerComponent>(sender)?.Mind;

		if (mindId != null)
		{
			var metaData = MetaData(sender);
			// Generate a random single-word chant from cult-chants.ftl
			var chant = GenerateChant(wordCount: 1);
			_chat.TrySendInGameICMessage(sender, chant, InGameICChatType.Whisper, ChatTransmitRange.Normal);
			_jobs.MindTryGetJob(mindId, out var prototype);
			string job = "Crewmember";
			if (prototype != null)
				job = prototype.LocalizedName;
			AnnounceToCultists(message = Loc.GetString("cult-commune-message", ("name", metaData.EntityName),
				("job", job), ("message", formattedMessage)), color:new Color(166, 27, 27, 255),
				fontSize: 12, newlineNeeded:false, includeGhosts:true);
		}
	}

	/// <summary>
	/// Adds blood to the ritual pool when someone is converted.
	/// Caps blood at the current stage threshold to prevent over-collection.
	/// </summary>
	public void AddBloodForConversion(double amount = 100.0)
	{
		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
		{
			// Determine the current stage cap
			double currentCap = ruleComp.BloodRequiredForVeil;
			if (!ruleComp.HasEyes)
				currentCap = ruleComp.BloodRequiredForEyes;
			else if (!ruleComp.HasRisen)
				currentCap = ruleComp.BloodRequiredForRise;
			else if (!ruleComp.VeilWeakened)
				currentCap = ruleComp.BloodRequiredForRise; // Cap at Rise threshold until Tear Veil ritual is done
			
			// Add blood but don't exceed the current stage cap
			ruleComp.BloodCollected = Math.Min(ruleComp.BloodCollected + amount, currentCap);
			// BloodCultRuleComponent is server-only and doesn't need to be dirtied
			return;
		}
	}

	/// <summary>
	/// Progresses the cult to stage 3 (Veil Weakened) when the Tear the Veil ritual is completed.
	/// Sets up the final summoning ritual site.
	/// </summary>
	public void CompleteVeilRitual()
	{
		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
		{
			if (!ruleComp.HasRisen)
				return; // Can't complete the ritual before reaching stage 2

			if (!ruleComp.VeilWeakened)
			{
				ruleComp.VeilWeakened = true;

				// Set up the final summoning ritual site
				var riftSetup = EntityManager.System<BloodCult.EntitySystems.BloodCultRiftSetupSystem>();
				var rift = riftSetup.TrySetupRitualSite(ruleComp);

				if (rift != null)
				{
					// Announce rift creation to all cultists
					var cultists = GetCultists();
					
					// Give cultists their halos now that the veil has been torn
					RiseCultists(cultists);
					
					foreach (var cultist in cultists)
					{
						_popupSystem.PopupEntity(
							Loc.GetString("cult-rift-spawned"),
							cultist, cultist, PopupType.LargeCaution
						);
					}
				}

				// Announcement will be handled in ActiveTick
			}
			return;
		}
	}

	/// <summary>
	/// Announces Nar'Sie's summon to the entire station and triggers end-game events.
	/// </summary>
	public void AnnounceNarsieSummon()
	{
		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
		{
			ruleComp.CultistsWin = true;
			ruleComp.CultVictoryEndTime = _timing.CurTime + ruleComp.CultVictoryEndDelay;

			// Station-wide announcement
			_chat.DispatchGlobalAnnouncement(
				Loc.GetString("cult-narsie-spawning"),
				colorOverride: Color.DarkRed
			);

			return;
		}
	}

	#region Admin Commands

	[AdminCommand(AdminFlags.Fun)]
	private void QueryBloodCommand(IConsoleShell shell, string argstr, string[] args)
	{
		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		var found = false;

		while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
		{
			if (!GameTicker.IsGameRuleActive(uid, gameRule))
				continue;

			found = true;
			var currentBlood = Math.Round(ruleComp.BloodCollected, 1);
			var eyesRequired = Math.Round(ruleComp.BloodRequiredForEyes, 1);
			var riseRequired = Math.Round(ruleComp.BloodRequiredForRise, 1);
			var veilRequired = Math.Round(ruleComp.BloodRequiredForVeil, 1);

			shell.WriteLine($"=== Blood Cult Status ===");
			shell.WriteLine($"Current Blood Collected: {currentBlood}u");
			shell.WriteLine($"");
			shell.WriteLine($"Phase 1 (Eyes): {eyesRequired}u needed - {(ruleComp.HasEyes ? "COMPLETE" : $"{Math.Round(eyesRequired - currentBlood, 1)}u remaining")}");
			shell.WriteLine($"Phase 2 (Rise): {riseRequired}u needed - {(ruleComp.HasRisen ? "COMPLETE" : $"{Math.Round(riseRequired - currentBlood, 1)}u remaining")}");
			shell.WriteLine($"Phase 3 (Veil): {veilRequired}u needed - {(ruleComp.VeilWeakened ? "COMPLETE" : $"{Math.Round(veilRequired - currentBlood, 1)}u remaining")}");

			if (shell.Player != null)
			{
				_adminLogger.Add(LogType.Action, LogImpact.Low, 
					$"{shell.Player} queried blood cult status: {currentBlood}u collected");
			}
		}

		if (!found)
		{
			shell.WriteError("No active Blood Cult game rule found.");
		}
	}

	[AdminCommand(AdminFlags.Fun)]
	private void SetBloodCommand(IConsoleShell shell, string argstr, string[] args)
	{
		if (args.Length != 1)
		{
			shell.WriteError("Usage: cult_setblood <amount>");
			return;
		}

		if (!double.TryParse(args[0], out var amount))
		{
			shell.WriteError("Invalid amount. Must be a number.");
			return;
		}

		if (amount < 0)
		{
			shell.WriteError("Amount cannot be negative.");
			return;
		}

		var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		var found = false;

		while (query.MoveNext(out var uid, out var ruleComp, out var gameRule))
		{
			if (!GameTicker.IsGameRuleActive(uid, gameRule))
				continue;

			found = true;
			var oldAmount = ruleComp.BloodCollected;
			ruleComp.BloodCollected = amount;

			shell.WriteLine($"Blood cult amount set from {Math.Round(oldAmount, 1)}u to {Math.Round(amount, 1)}u");

			if (shell.Player != null)
			{
				_adminLogger.Add(LogType.Action, LogImpact.Medium, 
					$"{shell.Player} set blood cult amount from {oldAmount}u to {amount}u");
			}
		}

		if (!found)
		{
			shell.WriteError("No active Blood Cult game rule found.");
		}
	}

	#endregion
}
