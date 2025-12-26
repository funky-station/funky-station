// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later OR MIT

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Server.Audio;
using Content.Server.Camera;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Popups;
using Content.Server.Body.Systems;
using Content.Server.Mind;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Anomaly;
using Content.Shared.Audio;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Content.Shared.Ghost;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Mind.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Server.Player;
using Robust.Shared.Random;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;
using Robust.Shared.Maths;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Handles Blood Cult rift pulsing (adding Sanguine Perniculate) and final summoning ritual.
/// </summary>
public sealed partial class BloodCultRiftSystem : EntitySystem
{
	private const float ShakeRange = 25f;
	private const float SummoningRuneDetectionRange = 1.5f;
	private const float FinalRitualShakeIntensity = 9f;
	private static readonly float[] SacrificeChantDelays = { 15f, 10f, 7f, 5f, 3f, 1f };

	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
	[Dependency] private readonly EntityLookupSystem _lookup = default!;
	[Dependency] private readonly AppearanceSystem _appearance = default!;
	[Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;
	[Dependency] private readonly CameraRecoilSystem _cameraRecoil = default!;
	[Dependency] private readonly SharedTransformSystem _transformSystem = default!;
	[Dependency] private readonly IPlayerManager _playerManager = default!;
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly ExplosionSystem _explosionSystem = default!;
	[Dependency] private readonly BodySystem _bodySystem = default!;
	//[Dependency] private readonly MindSystem _mindSystem = default!;
	[Dependency] private readonly OfferOnTriggerSystem _offerSystem = default!;
	[Dependency] private readonly IGameTiming _timing = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;
	[Dependency] private readonly ServerGlobalSoundSystem _sound = default!;
	[Dependency] private readonly ChatSystem _chatSystem = default!;
	[Dependency] private readonly PuddleSystem _puddleSystem = default!;

	public override void Initialize()
	{
		base.Initialize();

		// Run before OfferOnTriggerSystem so ritual check happens first
		SubscribeLocalEvent<FinalSummoningRuneComponent, TriggerEvent>(OnTriggerRitual, before: new[] { typeof(OfferOnTriggerSystem) });
		SubscribeLocalEvent<BloodCultRiftComponent, TriggerEvent>(OnTriggerRitualFromRift, before: new[] { typeof(OfferOnTriggerSystem) });
	}

	public override void Update(float frameTime)
	{
		base.Update(frameTime);

		// Process all rifts
		// All uh... one of them. If there's more than one there's a bug. But hey, it could happen? Hopefully not.
		// But the EntityQueryEnumerator would break if we only had it handle one, so it works.
		var riftQuery = EntityQueryEnumerator<BloodCultRiftComponent, TransformComponent>();
		while (riftQuery.MoveNext(out var riftUid, out var riftComp, out var xform))
		{
			// Handle pulsing (adding Sanguine Perniculate)
			riftComp.TimeUntilNextPulse -= frameTime;
			if (riftComp.TimeUntilNextPulse <= 0)
			{
				PulseRift(riftUid, riftComp);
				riftComp.TimeUntilNextPulse = riftComp.PulseInterval;
			}

			// No longer needed - FinalRiftRune is permanent and always in range

			// Handle active ritual chanting
			if (riftComp.RitualInProgress)
			{
				riftComp.TimeUntilNextChant -= frameTime;
				if (riftComp.TimeUntilNextChant <= 0f)
				{
					ProcessChantStep(riftUid, riftComp, xform);
				}
			}
		}
	}

	/// <summary>
	/// Adds Sanguine Perniculate to the rift's solution and triggers visual/audio effects.
	/// </summary>
	private void PulseRift(EntityUid riftUid, BloodCultRiftComponent riftComp)
	{
		// If this check fails, something is very wrong. All rifts should have a solution container.
		if (!_solutionContainer.TryGetSolution(riftUid, "sanguine_pool", out var solutionEnt, out var solution))
			return;

		// Add Sanguine Perniculate and spill any excess
		var amount = FixedPoint2.New(riftComp.BloodPerPulse);
		if (_solutionContainer.TryAddReagent(solutionEnt.Value, "SanguinePerniculate", amount, out var accepted))
		{
			// Using fancy adding of reagents to overflow so it actually acts like a bucket. Better than just spawning it on the floor directly.
			var overflow = amount - accepted;
			if (overflow > FixedPoint2.Zero)
				SpillOverflow(riftUid, overflow);
		}
		else
		{
			SpillOverflow(riftUid, amount);
		}

		// Trigger pulse animation
		// This is actually the normal liquid anom animation. Why not use existing animations.
		if (TryComp<AppearanceComponent>(riftUid, out var appearance))
		{
			_appearance.SetData(riftUid, AnomalyVisualLayers.Animated, true, appearance);
			// Animation will auto-hide after animation completes
		}
	}

	private void DoShake(EntityUid riftUid, TransformComponent xform, float intensity)
	{
		// Not sure how far out this shakes it for. But I think it works pretty good?
		// May have to adjust later. It works at close range pretty well.
		// todo: Test how much the shake actually works from across the map
		// todo: I'm pretty sure the shake direction is wrong. It should be from the rift to the player.
		var epicenter = _transformSystem.ToMapCoordinates(xform.Coordinates);
		var filter = Filter.Empty();
		filter.AddInRange(epicenter, ShakeRange, _playerManager, EntityManager);

		foreach (var session in filter.Recipients)
		{
			if (session.AttachedEntity is not EntityUid uid)
				continue;

			var playerPos = _transformSystem.GetWorldPosition(uid);
			var delta = epicenter.Position - playerPos;
			if (delta.LengthSquared() < 0.0001f)
				delta = new Vector2(0.01f, 0f);

			var distance = delta.Length();
			var effect = intensity * (1 - distance / ShakeRange);
			if (effect <= 0f)
				continue;

			_cameraRecoil.KickCamera(uid, -Vector2.Normalize(delta) * effect);
		}

		// Play the blood sound. I couldn't find a better sound for this.
		// todo: Find a better sound
		_audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/Fluids/blood1.ogg"), riftUid, AudioParams.Default.WithVolume(-3f));
	}

	private void TryPerformFinalSacrifice(EntityUid riftUid, BloodCultRiftComponent component, TransformComponent xform)
	{
		// Make sure it actually needs to be done.
		if (component.FinalSacrificeDone || component.SacrificesCompleted >= component.RequiredSacrifices)
			return;

		var cultistsOnRunes = GetCultistsOnSummoningRunes(component);
			
		if (cultistsOnRunes.Count == 0)
		{
			component.ChantsCompletedInCycle = SacrificeChantDelays.Length;
			component.TimeUntilNextChant = 1f;
			return;
		}

		// If we haven't picked someone to lead the chant, pick someone. 
		// Whoever gets the fancy chant is the next to die. Hopefully that's spooky and ominous.
		// I want people thinking "Why am I saying something different from everyone else?"
		// Foreshadowing is fun *evil laugh*
		if (component.PendingSacrifice is not { } pending || !cultistsOnRunes.Contains(pending))
		{
			component.PendingSacrifice = _random.Pick(cultistsOnRunes);
		}

		// If the person who has the pending sacrifice flag doesn't exist, restart the chant.
		if (!TryComp(component.PendingSacrifice.Value, out TransformComponent? victimXform))
		{
			component.ChantsCompletedInCycle = SacrificeChantDelays.Length;
			component.TimeUntilNextChant = 1f;
			return;
		}

		// If the person who has the pending sacrifice flag isn't on a rune, restart the chant.
		if (!cultistsOnRunes.Contains(component.PendingSacrifice.Value))
		{
			component.PendingSacrifice = null;
			component.ChantsCompletedInCycle = SacrificeChantDelays.Length;
			component.TimeUntilNextChant = 1f;
			return;
		}

		// Kill the sacrifice and soulstone them. If it can't kill them, restart the chant.
		// This should never happen, because if the code gets this far it should be able to kill them.
		var victim = component.PendingSacrifice.Value;
		if (!_offerSystem.TryForceSoulstoneCreation(victim, victimXform.Coordinates))
		{
			component.ChantsCompletedInCycle = SacrificeChantDelays.Length;
			component.TimeUntilNextChant = 1f;
			return;
		}

		// Gib the body after the brain has been removed
		// Use the explode smite approach: queue an explosion and gib without organs
		// This prevents issues with organs that don't have ContainerManagerComponent
		if (Exists(victim))
		{
			var coords = _transformSystem.GetMapCoordinates(victim);
			_explosionSystem.QueueExplosion(coords, ExplosionSystem.DefaultExplosionPrototypeId,
				4, 1, 2, victim, maxTileBreak: 0);
			_bodySystem.GibBody(victim, gibOrgans: false);
		}

		//Increment the sacrifices, play an announcement, and reset the chant.
		component.SacrificesCompleted++;
		component.RequiredCultistsForChant = Math.Max(1, component.RequiredCultistsForChant - 1);
		component.PendingSacrifice = null;
		component.FinalSacrificePending = false;
		component.ChantsCompletedInCycle = 0;
		component.TimeUntilNextChant = 0f;

		AnnounceSacrificeProgress(component.SacrificesCompleted);

		if (component.SacrificesCompleted >= component.RequiredSacrifices)
		{
			component.FinalSacrificeDone = true;
		}
	}

	/// <summary>
	/// When a cultist clicks on the BloodCultRift itself, start the ritual.
	/// </summary>
	private void OnTriggerRitualFromRift(EntityUid uid, BloodCultRiftComponent component, TriggerEvent args)
	{
		// Only cultists can trigger the ritual
		if (args.User == null || !TryComp<BloodCultistComponent>(args.User, out var cultist))
		{
			args.Handled = true;
			return;
		}

		var user = args.User.Value;
		var riftUid = uid;

		// Don't start if ritual is already in progress
		if (component.RitualInProgress)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-already-in-progress"),
				user, user, PopupType.MediumCaution
			);
			args.Handled = true;
			return;
		}

		// Ensure the cult has weakened the veil (blood collected plus ritual)
		//Should never happen, since the rift spawns "after" the veil is weakened. But this covers weird use cases of admin-spawned rifts.
		if (!_bloodCultRule.TryGetActiveRule(out var ruleComp) || !ruleComp.VeilWeakened || ruleComp.BloodCollected < ruleComp.BloodRequiredForVeil)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-too-early",
					("collected", Math.Round(ruleComp.BloodCollected, 1)),
					("required", Math.Round(ruleComp.BloodRequiredForVeil, 1))),
				user, user, PopupType.LargeCaution
			);
			args.Handled = true;
			return;
		}

		component.RequiredCultistsForChant = 3;

		// Check if enough runes have cultists on them
		var cultistsOnRunes = GetCultistsOnSummoningRunes(component);
		if (cultistsOnRunes.Count < component.RequiredCultistsForChant)
		{
			var allowPopup = component.LastNotEnoughCultistsPopup == TimeSpan.Zero ||
				(_timing.CurTime - component.LastNotEnoughCultistsPopup) > TimeSpan.FromSeconds(1);

			if (allowPopup)
			{
				component.LastNotEnoughCultistsPopup = _timing.CurTime;
				_popupSystem.PopupEntity(
					Loc.GetString("cult-final-ritual-not-enough-cultists",
						("current", cultistsOnRunes.Count),
						("required", component.RequiredCultistsForChant)),
					user, user, PopupType.LargeCaution
				);
			}
			args.Handled = true;
			return;
		}

		// Ritual can begin
		component.RitualInProgress = true;
		component.LastNotEnoughCultistsPopup = _timing.CurTime;
		component.CurrentChantStep = 0;
		component.SacrificesCompleted = 0;
		component.RequiredSacrifices = 3;
		component.FinalSacrificeDone = false;
		component.TotalChantSteps = (SacrificeChantDelays.Length + 1) * component.RequiredSacrifices;
		component.ChantsCompletedInCycle = 0;
		component.PendingSacrifice = null;
		component.TimeUntilNextChant = 0f;
		component.FinalSacrificePending = false;
		component.TimeUntilNextShake = 0f;
		component.NextShakeIndex = 0;

		// Uses placeholder music for now.
		// todo: find better bloodcult music
		StartRitualMusic(riftUid, component);

		// Announce to all cultists
		AnnounceRitualStart();

		// Immediately perform first chant
		ProcessChantStep(riftUid, component, Transform(riftUid));

		args.Handled = true;
	}

	/// <summary>
	/// When a cultist triggers a final summoning rune, check if enough cultists are on all 3 runes.
	/// </summary>
	private void OnTriggerRitual(EntityUid uid, FinalSummoningRuneComponent finalRune, TriggerEvent args)
	{
		// Only cultists can trigger the ritual
		if (args.User == null || !TryComp<BloodCultistComponent>(args.User, out var cultist))
		{
			args.Handled = true;
			return;
		}

		var user = args.User.Value;

		// Get the rift component
		if (finalRune.RiftUid == null || !TryComp<BloodCultRiftComponent>(finalRune.RiftUid, out var component))
		{
			args.Handled = true;
			return;
		}

		var riftUid = finalRune.RiftUid.Value;

		// Don't start if ritual is already in progress
		if (component.RitualInProgress)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-already-in-progress"),
				user, user, PopupType.MediumCaution
			);
			args.Handled = true;
			return;
		}

		// Ensure the cult has weakened the veil (blood collected plus ritual)
		if (!_bloodCultRule.TryGetActiveRule(out var ruleComp) || !ruleComp.VeilWeakened || ruleComp.BloodCollected < ruleComp.BloodRequiredForVeil)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-too-early",
					("collected", Math.Round(ruleComp.BloodCollected, 1)),
					("required", Math.Round(ruleComp.BloodRequiredForVeil, 1))),
				user, user, PopupType.LargeCaution
			);
			args.Handled = true;
			return;
		}

		component.RequiredCultistsForChant = 3;

		// Check if enough runes have cultists on them
		var cultistsOnRunes = GetCultistsOnSummoningRunes(component);
		if (cultistsOnRunes.Count < component.RequiredCultistsForChant)
		{
			var allowPopup = component.LastNotEnoughCultistsPopup == TimeSpan.Zero ||
				(_timing.CurTime - component.LastNotEnoughCultistsPopup) > TimeSpan.FromSeconds(1);

			if (allowPopup)
			{
				component.LastNotEnoughCultistsPopup = _timing.CurTime;
				_popupSystem.PopupEntity(
					Loc.GetString("cult-final-ritual-not-enough-cultists",
						("current", cultistsOnRunes.Count),
						("required", component.RequiredCultistsForChant)),
					user, user, PopupType.LargeCaution
				);
			}
			args.Handled = true;
			return;
		}

		// Ritual can begin!
		component.RitualInProgress = true;
		component.LastNotEnoughCultistsPopup = _timing.CurTime;
		component.CurrentChantStep = 0;
		component.SacrificesCompleted = 0;
		component.RequiredSacrifices = 3;
		component.FinalSacrificeDone = false;
		component.TotalChantSteps = (SacrificeChantDelays.Length + 1) * component.RequiredSacrifices;
		component.ChantsCompletedInCycle = 0;
		component.PendingSacrifice = null;
		component.TimeUntilNextChant = 0f;
		component.FinalSacrificePending = false;
		component.TimeUntilNextShake = 0f;
		component.NextShakeIndex = 0;

		// Uses placeholder music for now.
		// todo: find better bloodcult music
		StartRitualMusic(riftUid, component);

		// Announce to all cultists
		AnnounceRitualStart();

		// Immediately perform first chant
		ProcessChantStep(riftUid, component, Transform(riftUid));

		args.Handled = true;
	}

	/// <summary>
	/// Processes a single chant step in the final ritual.
	/// </summary>
	private void ProcessChantStep(EntityUid runeUid, BloodCultRiftComponent component, TransformComponent xform)
	{
		// Count cultists on runes
		var cultistsOnRunes = GetCultistsOnSummoningRunes(component);
		var cultistCount = cultistsOnRunes.Count;

		if (cultistCount < component.RequiredCultistsForChant)
		{
			// Not enough cultists, ritual fails
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-not-enough-at-end",
					("current", cultistCount),
					("required", component.RequiredCultistsForChant)),
				runeUid, PopupType.LargeCaution
			);

			AnnounceRitualFailure();

			if (component.RitualMusicPlaying)
			{
				_sound.StopStationEventMusic(runeUid, StationEventMusicType.CosmicCult);
				component.RitualMusicPlaying = false;
			}

			component.RitualInProgress = false;
			component.CurrentChantStep = 0;
			component.TimeUntilNextChant = 0f;
			component.ShakeDelays.Clear();
			component.NextShakeIndex = 0;
			component.TimeUntilNextShake = 0f;
			component.FinalSacrificePending = false;
			component.FinalSacrificeDone = false;
			component.SacrificesCompleted = 0;
			component.PendingSacrifice = null;
			component.ChantsCompletedInCycle = 0;
			// Don't reset the required cultists for chant. If people are dead let them keep the counter going.
			//component.RequiredCultistsForChant = 3;
			return;
		}

		EnsurePendingSacrifice(component, cultistsOnRunes);

		var chant = _bloodCultRule.GenerateChant(wordCount: 4); // Longer chants for final ritual

		bool hasPendingSacrifice = component.PendingSacrifice != null;
		var pendingUid = component.PendingSacrifice;
		bool shouldPromoteAllToLeaders = !hasPendingSacrifice && cultistCount == 1;

		foreach (var cultist in cultistsOnRunes)
		{
			if (!Exists(cultist))
				continue;

			var text = (component.PendingSacrifice == cultist || shouldPromoteAllToLeaders) ? chant : "Nar'Sie!";
			_bloodCultRule.Speak(cultist, text, forceLoud: true);
		}

		DoShake(runeUid, xform, FinalRitualShakeIntensity);

		// Increment chant step
		component.CurrentChantStep++;
		component.ChantsCompletedInCycle++;

		if (component.ChantsCompletedInCycle > SacrificeChantDelays.Length)
		{
			TryPerformFinalSacrifice(runeUid, component, xform);

			// Ritual may have ended inside TryPerformFinalSacrifice
			if (!component.RitualInProgress)
				return;
		}
		else
		{
			component.TimeUntilNextChant = SacrificeChantDelays[component.ChantsCompletedInCycle - 1];
		}

		// Check if ritual is complete
		if (component.CurrentChantStep >= component.TotalChantSteps)
		{
			if (component.SacrificesCompleted >= component.RequiredSacrifices)
			{
				// SUCCESS! Summon Nar'Sie
				SummonNarsie(runeUid, xform);
				AnnounceRitualSuccess();
				// Stop the music, and make sure there's no shake or chanting ongoing.
				// The shake and chanting should stop on their own when Nar'Sie eats them, but just incase.
				if (component.RitualMusicPlaying)
				{
					_sound.StopStationEventMusic(runeUid, StationEventMusicType.CosmicCult);
					component.RitualMusicPlaying = false;
				}

				component.RitualInProgress = false;
				component.CurrentChantStep = 0;
				component.TimeUntilNextChant = 0f;
				component.ShakeDelays.Clear();
				component.NextShakeIndex = 0;
				component.TimeUntilNextShake = 0f;
				component.FinalSacrificePending = false;
				component.FinalSacrificeDone = false;
				component.PendingSacrifice = null;
				component.ChantsCompletedInCycle = 0;
			}
			else
			{
				component.CurrentChantStep = component.TotalChantSteps;
				component.TimeUntilNextChant = 1f;
			}
		}
	}

	/// <summary>
	/// Summons Nar'Sie at the rift location.
	/// </summary>
	private void SummonNarsie(EntityUid riftUid, TransformComponent xform)
	{
		var coordinates = xform.Coordinates;

		// Spawn Nar'Sie spawn animation
		var narsieSpawn = Spawn("MobNarsieSpawn", coordinates);

		// Mark all cultists as having summoned Nar'Sie
		// This probably is part of the reason the endgame counter doesn't work for how many cultists there are.
		// todo: fix the endgame counter
		var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
		while (cultistQuery.MoveNext(out var cultistUid, out var cultist))
		{
			cultist.NarsieSummoned = coordinates;
		}

		// Announce to station
		_bloodCultRule.AnnounceNarsieSummon();
	}

	/// <summary>
	/// Gets a list of all live cultists currently standing on the summoning runes.
	/// </summary>
	private List<EntityUid> GetCultistsOnSummoningRunes(BloodCultRiftComponent riftComp)
	{
		var cultists = new HashSet<EntityUid>();

		foreach (var runeUid in riftComp.SummoningRunes)
		{
			if (!Exists(runeUid) || !TryComp(runeUid, out TransformComponent? runeXform))
				continue;

			// Look for cultists near this rune
			// The FinalRiftRune is 3x3 tiles, so we need a range that covers from center to corner
			// 3 tiles = 3 meters, so diagonal distance from center to corner is ~2.12 meters
			// Using 2.0f to ensure we cover the entire 3x3 area with some margin
			var nearbyEntities = _lookup.GetEntitiesInRange(runeXform.Coordinates, 2.0f);
			foreach (var entity in nearbyEntities)
			{
				if (!IsValidSummoningParticipant(entity))
					continue;

				cultists.Add(entity);
			}
		}

		var result = cultists.ToList();
		result.RemoveAll(uid => HasComp<GhostComponent>(uid));
		return result;
	}


	private void EnsurePendingSacrifice(BloodCultRiftComponent component, List<EntityUid> cultists)
	{
		if (component.PendingSacrifice is { } pending && cultists.Contains(pending))
			return;

		if (cultists.Count == 0)
		{
			component.PendingSacrifice = null;
			return;
		}

		component.PendingSacrifice = _random.Pick(cultists);
	}

	private void StartRitualMusic(EntityUid riftUid, BloodCultRiftComponent component)
	{
		if (component.RitualMusicPlaying)
			return;

		var resolved = _audio.ResolveSound(component.RitualMusic);
		if (ResolvedSoundSpecifier.IsNullOrEmpty(resolved))
			return;
        // todo: fix this. It's not the right music. But I don't know of better music.
		_sound.DispatchStationEventMusic(riftUid, resolved, StationEventMusicType.Nuke);
		component.RitualMusicPlaying = true;
	}

	private void AnnounceSacrificeProgress(int completed)
	{
		string? message = completed switch
		{
			1 => "The first sacrifice is complete. Nar'Sie begins to enter our reality.",
			2 => "The second sacrifice is complete. The Geometer of Blood pries open the veil.",
			3 => "The final sacrifice is complete. She. Is. Here.",
			_ => null
		};

		if (message == null)
			return;

		_chatSystem.DispatchGlobalAnnouncement(message, "Unknown", playSound: true);
	}

	private bool IsValidSummoningParticipant(EntityUid entity)
	{
		if (!HasComp<BloodCultistComponent>(entity))
			return false;

		if (_mobState.IsDead(entity) || _mobState.IsCritical(entity))
			return false;

		if (!TryComp<MindContainerComponent>(entity, out var mind) || mind.Mind == null)
			return false;

		return true;
	}

	private void SpillOverflow(EntityUid riftUid, FixedPoint2 overflow)
	{
		if (overflow <= FixedPoint2.Zero)
			return;

		var solution = new Solution("SanguinePerniculate", overflow);
		_puddleSystem.TrySpillAt(Transform(riftUid).Coordinates, solution, out _);
	}

	private void AnnounceRitualStart()
	{
		var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
		while (cultistQuery.MoveNext(out var cultistUid, out var _))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-started"),
				cultistUid, cultistUid, PopupType.LargeCaution
			);
		}
	}

	private void AnnounceRitualFailure()
	{
		var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
		while (cultistQuery.MoveNext(out var cultistUid, out var _))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-failed"),
				cultistUid, cultistUid, PopupType.LargeCaution
			);
		}
	}

	private void AnnounceRitualSuccess()
	{
		var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
		while (cultistQuery.MoveNext(out var cultistUid, out var _))
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-success"),
				cultistUid, cultistUid, PopupType.LargeCaution
			);
		}
	}
}