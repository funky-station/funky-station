// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.GameObjects;
using Content.Shared.Interaction.Events;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mobs.Systems;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Mobs;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Content.Server.Roles;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Server.Body.Components;
using Content.Shared.Destructible;

namespace Content.Server.BloodCult.EntitySystems;

public sealed class SoulStoneSystem : EntitySystem
{
	[Dependency] private readonly MindSystem _mind = default!;
	[Dependency] private readonly PopupSystem _popupSystem = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly BloodCultRuleSystem _cultRuleSystem = default!;
	[Dependency] private readonly BloodCultConstructSystem _constructSystem = default!;
	[Dependency] private readonly MobStateSystem _mobState = default!;
	[Dependency] private readonly IEntityManager _entityManager = default!;
	[Dependency] private readonly DamageableSystem _damageable = default!;
	[Dependency] private readonly IPrototypeManager _prototypeManager = default!;
	[Dependency] private readonly RoleSystem _role = default!;
	[Dependency] private readonly BloodstreamSystem _bloodstream = default!;

	private EntityQuery<ShadeComponent> _shadeQuery;

	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<SoulStoneComponent, AfterInteractEvent>(OnTryCaptureSoul);
		SubscribeLocalEvent<SoulStoneComponent, UseInHandEvent>(OnUseInHand);
		SubscribeLocalEvent<ShadeComponent, MobStateChangedEvent>(OnShadeDeath);
		SubscribeLocalEvent<SoulStoneComponent, DestructionEventArgs>(OnSoulStoneDestroyed);

		_shadeQuery = GetEntityQuery<ShadeComponent>();
	}

	private void OnTryCaptureSoul(Entity<SoulStoneComponent> ent, ref AfterInteractEvent args)
	{
		if (args.Handled
			|| !args.CanReach
			|| !args.ClickLocation.IsValid(_entityManager)
			|| !TryComp<BloodCultistComponent>(args.User, out var cultist) // ensure user is cultist
			|| HasComp<ActiveDoAfterComponent>(args.User)
			|| args.Target == null)
			return;

		if (!_shadeQuery.HasComponent(args.Target))
		{
			_constructSystem.TryApplySoulStone(ent, ref args);
			return;
		}

		args.Handled = true;

		if (args.Target != null && !_mobState.IsDead((EntityUid)args.Target) && TryComp<MindContainerComponent>(args.Target, out var mindContainer))
		{
			if (mindContainer.Mind != null && TryComp<MindComponent>((EntityUid)mindContainer.Mind, out var mindComp))
			{
				var coordinates = Transform((EntityUid)args.Target).Coordinates;
				_mind.TransferTo((EntityUid)mindContainer.Mind, ent, mind:mindComp);
				_audioSystem.PlayPvs("/Audio/Magic/blink.ogg", coordinates);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-shade-recalled"),
					args.User, args.User, PopupType.SmallCaution
				);
				QueueDel(args.Target);
			}
		}
	}

	private void OnUseInHand(Entity<SoulStoneComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

		args.Handled = true;

		if (TryComp<BloodCultistComponent>(args.User, out var _) && TryComp<MindContainerComponent>(ent, out var mindContainer))
		{
			if (mindContainer.Mind != null && TryComp<MindComponent>((EntityUid)mindContainer.Mind, out var mindComp))
			{
				// Make the mind in the soulstone a cultist (without giving them cultist abilities yet)
				var mindId = (EntityUid)mindContainer.Mind;
				if (!_role.MindHasRole<BloodCultRoleComponent>(mindId))
				{
					_role.MindAddRole(mindId, "MindRoleCultist", mindComp);
				}
				
			// Damage the user for releasing the shade
			if (TryComp<DamageableComponent>(args.User, out var damageable))
			{
				var damage = new DamageSpecifier();
				
				// Deal significant slash damage (30 points)
				damage.DamageDict.Add("Slash", FixedPoint2.New(30));
				
				_damageable.TryChangeDamage(args.User, damage, ignoreResistances: false);
				
				// Add a very large bleed if the user has a bloodstream
				if (TryComp<BloodstreamComponent>(args.User, out var bloodstream))
				{
					// Add 10 units/second bleed - this is a massive bleed that will rapidly drain blood
					_bloodstream.TryModifyBleedAmount(args.User, 10.0f, bloodstream);
				}
			}
				
				var coordinates = Transform((EntityUid)args.User).Coordinates;
				var construct = Spawn("MobBloodCultShade", coordinates);
				_mind.TransferTo((EntityUid)mindContainer.Mind, construct, mind:mindComp);
				
				// Set the soulstone reference on the Shade so it knows where to return
				if (TryComp<ShadeComponent>(construct, out var shadeComp))
				{
					shadeComp.SourceSoulstone = ent;
				}
				
				_audioSystem.PlayPvs("/Audio/Magic/blink.ogg", coordinates);
				_popupSystem.PopupEntity(
					Loc.GetString("cult-shade-summoned"),
					args.User, args.User, PopupType.SmallCaution
				);
				string summonerName = _entityManager.GetComponent<MetaDataComponent>(args.User).EntityName;
				_cultRuleSystem.AnnounceToCultist(Loc.GetString("cult-shade-servant", ("name", summonerName)), construct);
			}
			else
			{
				_popupSystem.PopupEntity(
					Loc.GetString("cult-soulstone-empty"),
					args.User, args.User, PopupType.SmallCaution
				);
			}
		}
	}

	private void OnShadeDeath(Entity<ShadeComponent> shade, ref MobStateChangedEvent args)
	{
		// Only handle when the Shade dies
		if (args.NewMobState != MobState.Dead)
			return;

		// Check if the Shade has a source soulstone and a mind
		if (shade.Comp.SourceSoulstone == null)
			return;

		var soulstone = shade.Comp.SourceSoulstone.Value;
		
		// Verify the soulstone still exists
		if (!Exists(soulstone))
			return;

		// Get the Shade's mind
		EntityUid? mindId = CompOrNull<MindContainerComponent>(shade)?.Mind;
		if (mindId == null || !TryComp<MindComponent>(mindId, out var mindComp))
			return;

		// Transfer the mind back to the soulstone
		var coordinates = Transform(shade).Coordinates;
		_mind.TransferTo((EntityUid)mindId, soulstone, mind: mindComp);
		_audioSystem.PlayPvs("/Audio/Magic/blink.ogg", coordinates);
		
		// Delete the Shade entity
		QueueDel(shade);
	}

	private void OnSoulStoneDestroyed(Entity<SoulStoneComponent> soulstone, ref DestructionEventArgs args)
	{
		// Get the mind from the soulstone
		EntityUid? mindId = CompOrNull<MindContainerComponent>(soulstone)?.Mind;
		if (mindId == null || !TryComp<MindComponent>(mindId, out var mindComp))
			return;

		// Get the original entity prototype
		if (soulstone.Comp.OriginalEntityPrototype == null)
			return;

		var originalPrototype = soulstone.Comp.OriginalEntityPrototype.Value;

		// Spawn the original entity at the soulstone's location
		var coordinates = Transform(soulstone).Coordinates;
		var originalEntity = Spawn(originalPrototype, coordinates);

		// Transfer the mind to the original entity
		_mind.TransferTo((EntityUid)mindId, originalEntity, mind: mindComp);

		// Play breaking sound (glass break sound is appropriate)
		_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Effects/glass_break1.ogg"), coordinates);
		
		_popupSystem.PopupEntity(
			Loc.GetString("cult-soulstone-shattered"),
			soulstone, PopupType.MediumCaution
		);
	}
}

