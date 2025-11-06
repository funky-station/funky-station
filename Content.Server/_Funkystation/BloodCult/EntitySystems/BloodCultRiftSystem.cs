// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.BloodCult.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.BloodCult;
using Content.Server.Explosion.EntitySystems;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Server.GameObjects;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Handles Blood Cult rift pulsing (adding Sanguine Perniculate) and final summoning ritual.
/// </summary>
public sealed partial class BloodCultRiftSystem : EntitySystem
{
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
	[Dependency] private readonly EntityLookupSystem _lookup = default!;
	[Dependency] private readonly AppearanceSystem _appearance = default!;
	[Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<FinalSummoningRuneComponent, TriggerEvent>(OnTriggerRitual);
	}

	public override void Update(float frameTime)
	{
		base.Update(frameTime);

		// Process all rifts
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

			// Handle active ritual chanting
			if (riftComp.RitualInProgress)
			{
				riftComp.TimeUntilNextChant -= frameTime;
				if (riftComp.TimeUntilNextChant <= 0)
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
		if (!_solutionContainer.TryGetSolution(riftUid, "sanguine_pool", out var solutionEnt, out var solution))
			return;

		// Add Sanguine Perniculate
		var amount = FixedPoint2.New(riftComp.BloodPerPulse);
		_solutionContainer.TryAddReagent(solutionEnt.Value, "SanguinePerniculate", amount);

		// Trigger pulse animation
		if (TryComp<AppearanceComponent>(riftUid, out var appearance))
		{
			_appearance.SetData(riftUid, AnomalyVisualLayers.Animated, true, appearance);
			// Animation will auto-hide after animation completes
		}
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

		// Check if all 3 runes have cultists on them
		var cultistsOnRunes = GetCultistsOnSummoningRunes(component);
		if (cultistsOnRunes.Count < 3)
		{
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-not-enough-cultists",
					("current", cultistsOnRunes.Count),
					("required", 3)),
				user, user, PopupType.LargeCaution
			);
			args.Handled = true;
			return;
		}

		// Ritual can begin!
		component.RitualInProgress = true;
		component.CurrentChantStep = 0;
		component.TimeUntilNextChant = component.ChantInterval;

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

		if (cultistCount < 3)
		{
			// Not enough cultists, ritual fails
			_popupSystem.PopupEntity(
				Loc.GetString("cult-final-ritual-not-enough-at-end",
					("current", cultistCount),
					("required", 3)),
				runeUid, PopupType.LargeCaution
			);

			AnnounceRitualFailure();
			EndRitual(component, false);
			return;
		}

		// Make all cultists on runes chant
		var chant = _bloodCultRule.GenerateChant(wordCount: 4); // Longer chants for final ritual
		foreach (var cultist in cultistsOnRunes)
		{
			if (Exists(cultist))
			{
				_bloodCultRule.Speak(cultist, chant);
			}
		}

		// Increment chant step
		component.CurrentChantStep++;

		// Check if ritual is complete
		if (component.CurrentChantStep >= component.TotalChantSteps)
		{
			// SUCCESS! Summon Nar'Sie
			SummonNarsie(runeUid, xform);
			AnnounceRitualSuccess();
			EndRitual(component, true);
		}
		else
		{
			// Schedule next chant
			component.TimeUntilNextChant = component.ChantInterval;
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
		var cultists = new List<EntityUid>();

		foreach (var runeUid in riftComp.SummoningRunes)
		{
			if (!Exists(runeUid) || !TryComp<TransformComponent>(runeUid, out var runeXform))
				continue;

			// Look for cultists near this rune
			var nearbyEntities = _lookup.GetEntitiesInRange(runeXform.Coordinates, 0.5f);
			foreach (var entity in nearbyEntities)
			{
				// Check if this is a live cultist or construct
				if ((HasComp<BloodCultistComponent>(entity) || HasComp<BloodCultConstructComponent>(entity))
					&& !_mobState.IsDead(entity))
				{
					cultists.Add(entity);
					break; // Only count one per rune
				}
			}
		}

		return cultists;
	}

	/// <summary>
	/// Ends the ritual and resets its state.
	/// </summary>
	private void EndRitual(BloodCultRiftComponent component, bool success)
	{
		component.RitualInProgress = false;
		component.CurrentChantStep = 0;
		component.TimeUntilNextChant = 0f;
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

/// <summary>
/// Anomaly visual layers for the rift pulsing animation.
/// </summary>
public enum AnomalyVisualLayers : byte
{
	Base,
	Animated
}

