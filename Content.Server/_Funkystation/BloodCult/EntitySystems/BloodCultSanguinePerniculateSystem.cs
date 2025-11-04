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
/// Handles Sanguine Perniculate touch reactions for blood cultists.
/// When a cultist touches Sanguine Perniculate, it heals their holy damage.
/// </summary>
public sealed class BloodCultSanguinePerniculateSystem : EntitySystem
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
		// Only process Sanguine Perniculate
		if (args.Reagent.ID != "SanguinePerniculate")
			return;

		// Only process touch reactions (splashing)
		if (args.Method != ReactionMethod.Touch)
			return;

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
}

