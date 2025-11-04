// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio.Systems;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Content.Shared.Interaction.Events;
using Content.Server.Mind;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Popups;
using Content.Server.Popups;
using Content.Shared.Interaction;
using Content.Shared.DoAfter;
using Content.Server.GameTicking.Rules;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Content.Server.Ghost.Roles.Components;

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
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	[ValidatePrototypeId<DamageTypePrototype>] private const string BloodlossDamage = "Bloodloss";
	[ValidatePrototypeId<DamageTypePrototype>] private const string IonDamage = "Ion";
	[ValidatePrototypeId<DamageTypePrototype>] private const string SlashDamage = "Slash";

	private EntityQuery<ShadeComponent> _shadeQuery;

	public override void Initialize()
    {
        base.Initialize();

		SubscribeLocalEvent<SoulStoneComponent, AfterInteractEvent>(OnTryCaptureSoul);
		SubscribeLocalEvent<SoulStoneComponent, UseInHandEvent>(OnUseInHand);
		SubscribeLocalEvent<SoulStoneComponent, MindRemovedMessage>(OnSoulstoneMindRemoved);
		SubscribeLocalEvent<ShadeComponent, MobStateChangedEvent>(OnShadeDeath);

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
			var coordinates = Transform((EntityUid)args.User).Coordinates;
			var construct = Spawn("MobBloodCultShade", coordinates);
			_mind.TransferTo((EntityUid)mindContainer.Mind, construct, mind:mindComp);
			
			// Link the shade back to the soulstone so it can return on death
			if (TryComp<ShadeComponent>(construct, out var shadeComp))
			{
				shadeComp.OriginSoulstone = ent;
			}
			
		// Apply bloodloss damage to the user for summoning
		if (TryComp<DamageableComponent>(args.User, out var damageable))
		{
			string damageType;
			if (damageable.Damage.DamageDict.ContainsKey(BloodlossDamage))
				damageType = BloodlossDamage;
			else if (damageable.Damage.DamageDict.ContainsKey(IonDamage))
				damageType = IonDamage;
			else
				damageType = SlashDamage;
			
			var bloodDamage = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>(damageType), FixedPoint2.New(15));
			_damageableSystem.TryChangeDamage(args.User, bloodDamage, true, origin: args.User);
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

	private void OnSoulstoneMindRemoved(EntityUid uid, SoulStoneComponent component, MindRemovedMessage args)
	{
		// When a player ghosts out of a soulstone, make it available for ghost takeover
		if (!HasComp<GhostRoleComponent>(uid))
		{
			EnsureComp<GhostTakeoverAvailableComponent>(uid);
			
			var ghostRole = EnsureComp<GhostRoleComponent>(uid);
			ghostRole.RoleName = Loc.GetString("cult-soulstone-role-name");
			ghostRole.RoleDescription = Loc.GetString("cult-soulstone-role-description");
			ghostRole.RoleRules = Loc.GetString("cult-soulstone-role-rules");
		}
	}

	private void OnShadeDeath(EntityUid uid, ShadeComponent component, MobStateChangedEvent args)
	{
		// Only handle when the shade dies
		if (!_mobState.IsDead(uid))
			return;

		// Check if the shade has an origin soulstone
		if (component.OriginSoulstone == null || !EntityManager.EntityExists(component.OriginSoulstone))
			return;

		// Transfer the mind back to the soulstone
		if (TryComp<MindContainerComponent>(uid, out var mindContainer) && 
			mindContainer.Mind != null && 
			TryComp<MindComponent>((EntityUid)mindContainer.Mind, out var mindComp))
		{
			var soulstone = (EntityUid)component.OriginSoulstone;
			_mind.TransferTo((EntityUid)mindContainer.Mind, soulstone, mind: mindComp);
			
			_popupSystem.PopupEntity(
				Loc.GetString("cult-shade-death-return"),
				soulstone, PopupType.Medium
			);
		}
	}
}
