// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Popups;
using Content.Shared.BloodCult;
using Content.Shared.Chemistry;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;

namespace Content.Server.BloodCult.EntitySystems;

/// <summary>
/// Handles HolyWater touch reactions for blood cultists.
/// When a cultist is splashed with holy water, they take additional holy damage.
/// </summary>
public sealed class BloodCultHolyWaterSystem : EntitySystem
{
	[Dependency] private readonly DamageableSystem _damageable = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audio = default!;

	public override void Initialize()
	{
		base.Initialize();

		// Subscribe to reagent reactions on cultists
		SubscribeLocalEvent<BloodCultistComponent, ReactionEntityEvent>(OnCultistReaction);
	}

	private void OnCultistReaction(EntityUid uid, BloodCultistComponent component, ref ReactionEntityEvent args)
	{
		// Only process Holywater
		if (args.Reagent.ID != "Holywater")
			return;

		// Only process touch reactions (splashing/spraying)
		if (args.Method != ReactionMethod.Touch)
			return;

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

