// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Linq;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;
using Content.Server.Popups;
using Content.Shared.Popups;
using Content.Shared.BloodCult.Components;
using Content.Shared.Mobs.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.BloodCult;
using Content.Server.Explosion.EntitySystems;

namespace Content.Server.BloodCult.EntitySystems
{
	/// <summary>
	/// Handles the Tear the Veil ritual that progresses the cult to stage 3 (Veil Weakened).
	/// Requires multiple cultists to stand on TearVeilRunes and chant together.
	/// </summary>
	public sealed partial class TearVeilSystem : EntitySystem
	{
		[Dependency] private readonly PopupSystem _popupSystem = default!;
		[Dependency] private readonly MobStateSystem _mobState = default!;
		[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
		[Dependency] private readonly EntityLookupSystem _lookup = default!;

		public override void Initialize()
		{
			base.Initialize();

			SubscribeLocalEvent<TearVeilComponent, TriggerEvent>(OnTriggerRitual);
		}

		public override void Update(float frameTime)
		{
			base.Update(frameTime);

			// Process all active rituals
			var ritualQuery = EntityQueryEnumerator<TearVeilComponent, TransformComponent>();
			while (ritualQuery.MoveNext(out var runeUid, out var component, out var xform))
			{
				if (!component.RitualInProgress)
					continue;

				// Decrement time until next chant
				component.TimeUntilNextChant -= frameTime;

				if (component.TimeUntilNextChant <= 0)
				{
					// Time to perform the next chant step
					ProcessChantStep(runeUid, component, xform);
				}
			}
		}

	/// <summary>
	/// When a cultist triggers a TearVeilRune, check if there are enough cultists on runes and start the ritual.
	/// </summary>
	private void OnTriggerRitual(EntityUid uid, TearVeilComponent component, TriggerEvent args)
	{
		// Only cultists can trigger the ritual
		if (args.User == null || !TryComp<BloodCultistComponent>(args.User, out var cultist))
		{
			args.Handled = true;
			return;
		}

		var user = args.User.Value;

		// Check if a ritual is already in progress anywhere
		var existingRitualQuery = EntityQueryEnumerator<TearVeilComponent>();
		while (existingRitualQuery.MoveNext(out var existingUid, out var existingComp))
		{
			if (existingComp.RitualInProgress)
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-ritual-already-in-progress"),
					user, user, PopupType.MediumCaution
				);
				args.Handled = true;
				return;
			}
		}

		// Get the map this rune is on. Make sure the rune is valid before using it later in the function.
		if (!TryComp<TransformComponent>(uid, out var xform) || !TryComp<MapGridComponent>(xform.GridUid, out var _))
		{
			args.Handled = true;
			return;
		}

		//If the map itself isn't valid, don't continue.
		var mapUid = xform.MapUid ?? EntityUid.Invalid;
		if (!mapUid.IsValid())
		{
			args.Handled = true;
			return;
		}

	// Get the minimum cultists required from the game rule
	var query = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
	BloodCultRuleComponent? cultRule = null;
	while (query.MoveNext(out _, out var ruleComp, out _))
	{
		cultRule = ruleComp;
		break;
	}
	
	if (cultRule == null)
	{
		args.Handled = true;
		return;
	}
	
	var minimumRequired = cultRule.MinimumCultistsForVeilRitual;

	// Count how many cultists are currently standing on TearVeilRunes on this map
	var cultistsOnRunes = CountCultistsOnRunes(mapUid);

	if (cultistsOnRunes < minimumRequired)
	{
		_popupSystem.PopupEntity(
			Loc.GetString("cult-veil-ritual-not-enough-cultists", 
				("current", cultistsOnRunes), 
				("required", minimumRequired)),
			user, user, PopupType.LargeCaution
		);
		args.Handled = true;
		return;
	}

	// Ritual can begin!
	component.RitualInProgress = true;
	component.RitualMapUid = mapUid;
	component.CurrentChantStep = 0;
	component.TimeUntilNextChant = component.ChantInterval;

	// Announce to all cultists that the ritual has begun
	AnnounceRitualStart(minimumRequired);

		// Immediately perform the first chant
		ProcessChantStep(uid, component, xform);

		args.Handled = true;
	}

	/// <summary>
	/// Processes a single chant step in the ritual.
	/// </summary>
	private void ProcessChantStep(EntityUid runeUid, TearVeilComponent component, TransformComponent xform)
	{
		if (component.RitualMapUid == null || !component.RitualMapUid.Value.IsValid())
		{
			EndRitual(component, false);
			return;
		}

		// Get the minimum cultists required from the game rule
		var ruleQuery = EntityQueryEnumerator<BloodCultRuleComponent, GameRuleComponent>();
		BloodCultRuleComponent? cultRule = null;
		while (ruleQuery.MoveNext(out _, out var ruleComp, out _))
		{
			cultRule = ruleComp;
			break;
		}
		
		if (cultRule == null)
		{
			EndRitual(component, false);
			return;
		}
		
		var minimumRequired = cultRule.MinimumCultistsForVeilRitual;

		// Count cultists on runes
		var cultistsOnRunes = GetCultistsOnRunes(component.RitualMapUid.Value);
		var cultistCount = cultistsOnRunes.Count;

		if (cultistCount < minimumRequired)
		{
			// Not enough cultists, ritual fails
			_popupSystem.PopupEntity(
				Loc.GetString("cult-veil-ritual-not-enough-at-end", 
					("current", cultistCount), 
					("required", minimumRequired)),
				runeUid, PopupType.LargeCaution
			);

			AnnounceRitualFailure();
			EndRitual(component, false);
			return;
		}

			// Make all cultists on runes chant
			var chant = _bloodCultRule.GenerateChant(wordCount: 3);
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
				// SUCCESS! Progress the cult to stage 3
				_bloodCultRule.CompleteVeilRitual();
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
		/// Ends the ritual and resets its state.
		/// </summary>
		private void EndRitual(TearVeilComponent component, bool success)
		{
			component.RitualInProgress = false;
			component.RitualMapUid = null;
			component.CurrentChantStep = 0;
			component.TimeUntilNextChant = 0f;
				}

		/// <summary>
		/// Counts how many live cultists are currently standing on TearVeilRunes on the specified map.
		/// </summary>
		private int CountCultistsOnRunes(EntityUid mapUid)
		{
			var count = 0;
			var runeQuery = EntityQueryEnumerator<TearVeilComponent, TransformComponent>();

			while (runeQuery.MoveNext(out var runeUid, out var _, out var runeXform))
				{
				// Only count runes on the same map
				if (runeXform.MapUid != mapUid)
					continue;

				// Look for cultists near this rune
				var nearbyEntities = _lookup.GetEntitiesInRange(runeXform.Coordinates, 0.5f);
				foreach (var entity in nearbyEntities)
				{
					// Check if this is a live cultist or construct
					if ((HasComp<BloodCultistComponent>(entity) || HasComp<BloodCultConstructComponent>(entity)) 
						&& !_mobState.IsDead(entity))
				{
						count++;
						break; // Only count once per rune
				}
			}
			}

			return count;
		}

		/// <summary>
		/// Gets a list of all live cultists currently standing on TearVeilRunes on the specified map.
		/// </summary>
		private List<EntityUid> GetCultistsOnRunes(EntityUid mapUid)
		{
			var cultists = new List<EntityUid>();
			var runeQuery = EntityQueryEnumerator<TearVeilComponent, TransformComponent>();

			while (runeQuery.MoveNext(out var runeUid, out var _, out var runeXform))
			{
				// Only count runes on the same map
				if (runeXform.MapUid != mapUid)
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
						break; // Only add once per rune
				}
			}
			}

			return cultists;
		}

		/// <summary>
		/// Announces to all cultists that the Tear the Veil ritual has begun.
		/// </summary>
		private void AnnounceRitualStart(int requiredCultists)
		{
			var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
			while (cultistQuery.MoveNext(out var cultistUid, out var _))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-ritual-started", ("required", requiredCultists)),
					cultistUid, cultistUid, PopupType.LargeCaution
				);
				}
			}

		/// <summary>
		/// Announces to all cultists that the ritual has failed.
		/// </summary>
		private void AnnounceRitualFailure()
			{
			var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
			while (cultistQuery.MoveNext(out var cultistUid, out var _))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-ritual-failed"),
					cultistUid, cultistUid, PopupType.LargeCaution
				);
				}
			}

		/// <summary>
		/// Announces to all cultists that the ritual has succeeded.
		/// </summary>
		private void AnnounceRitualSuccess()
		{
			var cultistQuery = EntityQueryEnumerator<BloodCultistComponent>();
			while (cultistQuery.MoveNext(out var cultistUid, out var _))
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-veil-ritual-success"),
					cultistUid, cultistUid, PopupType.LargeCaution
				);
			}
		}
	}
}
