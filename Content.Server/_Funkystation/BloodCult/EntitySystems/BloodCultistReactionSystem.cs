// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Fluids.EntitySystems;
using Content.Server.GameTicking.Rules;
using Content.Server.Medical;
using Content.Server.Popups;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Unified system to handle all blood cultist reagent reactions.
/// This includes:
/// - Blood consumption (ingestion) -> vomits Sanguine Perniculate, adds to ritual pool
/// - Sanguine Perniculate (touch) -> heals holy damage
/// - Holy Water (touch) -> deals additional holy damage
/// </summary>
public sealed class BloodCultistReactionSystem : EntitySystem
{
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRule = default!;
	[Dependency] private readonly PuddleSystem _puddle = default!;
	[Dependency] private readonly VomitSystem _vomit = default!;
	[Dependency] private readonly DamageableSystem _damageable = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;

	public override void Initialize()
	{
		base.Initialize();

		// Single subscription to handle all cultist reagent reactions
		SubscribeLocalEvent<BloodCultistComponent, ReactionEntityEvent>(OnCultistReaction);
	}

	private void OnCultistReaction(EntityUid uid, BloodCultistComponent component, ref ReactionEntityEvent args)
	{
		// Handle blood ingestion
		if (args.Method == ReactionMethod.Ingestion)
		{
			HandleBloodIngestion(uid, ref args);
			return;
		}

		// Handle touch reactions
		if (args.Method == ReactionMethod.Touch)
		{
			// Check for Sanguine Perniculate healing
			if (args.Reagent.ID == "SanguinePerniculate")
			{
				HandleSanguinePerniculateTouch(uid, ref args);
			}
			// Check for Holy Water damage
			else if (args.Reagent.ID == "Holywater")
			{
				HandleHolyWaterTouch(uid, ref args);
			}
		}
	}

	/// <summary>
	/// Handles blood consumption by cultists.
	/// When cultists ingest blood, they vomit Sanguine Perniculate and add to the ritual pool.
	/// </summary>
	private void HandleBloodIngestion(EntityUid uid, ref ReactionEntityEvent args)
	{
		// Only process sacrifice blood reagents (not Sanguine Perniculate)
		if (!BloodCultConstants.SacrificeBloodReagents.Contains(args.Reagent.ID))
			return;

		var bloodAmount = args.ReagentQuantity.Quantity;
		if (bloodAmount <= 0)
			return;

		// Add to the global ritual pool
		_bloodCultRule.AddBloodForConversion(bloodAmount.Float());

		// Make the cultist vomit Sanguine Perniculate
		var vomitSolution = new Solution();
		vomitSolution.AddReagent(new ReagentId("SanguinePerniculate", null), bloodAmount);
		_puddle.TrySpillAt(Transform(uid).Coordinates, vomitSolution, out _);

		// Trigger vomit effects (slowdown, popup)
		_vomit.Vomit(uid, thirstAdded: -bloodAmount.Float() / 2, hungerAdded: -bloodAmount.Float() / 2);
	}

	/// <summary>
	/// Handles Sanguine Perniculate touch reactions for blood cultists.
	/// When a cultist touches Sanguine Perniculate, it heals their holy damage.
	/// </summary>
	private void HandleSanguinePerniculateTouch(EntityUid uid, ref ReactionEntityEvent args)
	{
		// Check if the cultist has any holy damage
		if (!TryComp<DamageableComponent>(uid, out var damageable))
			return;

		// Get the amount of holy damage
		if (!damageable.Damage.DamageDict.TryGetValue("Holy", out var holyDamage) || holyDamage <= 0)
			return;

		// Calculate healing amount based on Sanguine Perniculate quantity
		// 5u of Sanguine Perniculate heals 1 point of holy damage
		var healAmount = args.ReagentQuantity.Quantity.Float() / 5.0f;
		if (healAmount <= 0)
			return;

		// Don't heal more than the actual holy damage
		healAmount = Math.Min(healAmount, holyDamage.Float());

		// Heal the holy damage
		var healSpec = new DamageSpecifier();
		healSpec.DamageDict.Add("Holy", FixedPoint2.New(-healAmount));
		_damageable.TryChangeDamage(uid, healSpec, false, false, damageable);

		// Visual and audio feedback
		_popup.PopupEntity(
			Loc.GetString("cult-sanguine-perniculate-heal", ("amount", Math.Round(healAmount, 1))),
			uid, uid, PopupType.Medium
		);

		_audio.PlayPvs(
			new SoundPathSpecifier("/Audio/Effects/lightburn.ogg"),
			Transform(uid).Coordinates
		);
	}

	/// <summary>
	/// Handles HolyWater touch reactions for blood cultists.
	/// When a cultist is splashed with holy water, they take additional holy damage.
	/// </summary>
	private void HandleHolyWaterTouch(EntityUid uid, ref ReactionEntityEvent args)
	{
		// Calculate damage amount based on HolyWater quantity
		// The reagent already does 0.5 Holy damage per unit via its reactiveEffects
		// We add an additional 1.0 Holy damage per unit for cultists (total: 1.5 per unit)
		var damageAmount = args.ReagentQuantity.Quantity.Float() * 1.0f;
		if (damageAmount <= 0)
			return;

		// Apply additional holy damage to the cultist
		var damageSpec = new DamageSpecifier();
		damageSpec.DamageDict.Add("Holy", FixedPoint2.New(damageAmount));
		_damageable.TryChangeDamage(uid, damageSpec, false, true);

		// Visual and audio feedback
		_popup.PopupEntity(
			Loc.GetString("cult-holywater-burn", ("amount", Math.Round(damageAmount + args.ReagentQuantity.Quantity.Float() * 0.5f, 1))),
			uid, uid, PopupType.LargeCaution
		);

		_audio.PlayPvs(
			new SoundPathSpecifier("/Audio/Effects/lightburn.ogg"),
			Transform(uid).Coordinates
		);
	}
}

