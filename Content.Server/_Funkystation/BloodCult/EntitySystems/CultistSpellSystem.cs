// SPDX-FileCopyrightText: 2025 Skye <57879983+Rainbeon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 kbarkevich <24629810+kbarkevich@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Content.Server.Power.Components;
using Content.Shared.FixedPoint;
using Content.Server.Popups;
using Content.Server.Hands.Systems;
using Content.Shared.Actions;
using Content.Shared.Stacks;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Prototypes;
using Content.Server.BloodCult.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.DoAfter;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.StatusEffect;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Content.Shared.Emp;
using Content.Server.Emp;
using Content.Shared.Popups;
using Content.Server.PowerCell;
using Content.Shared.PowerCell;
using Content.Shared.PowerCell.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;


namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class CultistSpellSystem : EntitySystem
{
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly IPrototypeManager _proto = default!;
	[Dependency] private readonly SharedActionsSystem _action = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly StatusEffectsSystem _statusEffect = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRules = default!;
	[Dependency] private readonly HandsSystem _hands = default!;
	[Dependency] private readonly StaminaSystem _stamina = default!;
	[Dependency] private readonly EmpSystem _emp = default!;
	[Dependency] private readonly PowerCellSystem _powerCell = default!;
	[Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
	[Dependency] private readonly SharedTransformSystem _transform = default!;
	[Dependency] private readonly MapSystem _mapSystem = default!;
	[Dependency] private readonly IMapManager _mapManager = default!;
	[Dependency] private readonly IGameTiming _gameTiming = default!;
	[Dependency] private readonly IEntityManager _entMan = default!;
	[Dependency] private readonly SharedStunSystem _stun = default!;
	
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	private EntityQuery<EmpowerOnStandComponent> _runeQuery;

	private static string[] AvailableDaggers = ["CultDaggerCurved", "CultDaggerSerrated", "CultDaggerStraight"];

	public override void Initialize()
	{
		base.Initialize();

		_runeQuery = GetEntityQuery<EmpowerOnStandComponent>();

		SubscribeLocalEvent<BloodCultistComponent, SpellsMessage>(OnSpellSelectedMessage);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistSummonDagger>(OnSummonDagger);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistStudyVeil>(OnStudyVeil);
		SubscribeLocalEvent<BloodCultistComponent, BloodCultCommuneSendMessage>(OnCommune);
		SubscribeLocalEvent<BloodCultistComponent, EventCultistStun>(OnStun);
		SubscribeLocalEvent<CultMarkedComponent, AttackedEvent>(OnMarkedAttacked);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistTwistedConstruction>(OnTwistedConstruction);

		SubscribeLocalEvent<BloodCultistComponent, CarveSpellDoAfterEvent>(OnCarveSpellDoAfter);
	}

	private bool TryUseAbility(Entity<BloodCultistComponent> ent, BaseActionEvent args)
	{
		if (args.Handled)
            return false;
		if (!TryComp<CultistSpellComponent>(args.Action, out var actionComp))
            return false;

		// check if enough charges remain
		if (!actionComp.Infinite)
			actionComp.Charges = actionComp.Charges - 1;

		if (actionComp.Charges == 0)
		{
			_action.RemoveAction(args.Action.Owner);
			RemoveSpell(GetSpell(actionComp.AbilityId), ent.Comp);
		}

		// apply damage
		if (actionComp.HealthCost > 0 && TryComp<DamageableComponent>(ent, out var damageable))
		{
			DamageSpecifier appliedDamageSpecifier;
			if (damageable.Damage.DamageDict.ContainsKey("Bloodloss"))
				appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Bloodloss"), FixedPoint2.New(actionComp.HealthCost));
			else if (damageable.Damage.DamageDict.ContainsKey("Ion"))
				appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Ion"), FixedPoint2.New(actionComp.HealthCost));
			else
				appliedDamageSpecifier = new DamageSpecifier(_protoMan.Index<DamageTypePrototype>("Slash"), FixedPoint2.New(actionComp.HealthCost));
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
		}

		// verbalize invocation
		_bloodCultRules.Speak(ent, actionComp.Invocation);

		// play sound
		if (actionComp.CastSound != null)
			_audioSystem.PlayPvs(actionComp.CastSound, ent);

		return true;
	}

	public CultAbilityPrototype GetSpell(ProtoId<CultAbilityPrototype> id)
		=> _proto.Index(id);

	public void AddSpell(EntityUid uid, BloodCultistComponent comp, ProtoId<CultAbilityPrototype> id, bool recordKnownSpell = true)
	{
		var data = GetSpell(id);

		bool standingOnRune = false;
		var coords = new EntityCoordinates(uid, default);//.Position;
		var location = coords.AlignWithClosestGridTile(entityManager: EntityManager, mapManager: _mapManager);
		var gridUid = _transform.GetGrid(location);
		if (TryComp<MapGridComponent>(gridUid, out var grid))
		{
			var targetTile = _mapSystem.GetTileRef(gridUid.Value, grid, location);
			foreach(var possibleEnt in _mapSystem.GetAnchoredEntities(gridUid.Value, grid, targetTile.GridIndices))
			{
				if (_runeQuery.HasComponent(possibleEnt))
				{
					standingOnRune = true;
				}
			}
		}

	// If standing on rune, limit is 3 spells. If not on rune, forget all previous spells to learn new one
	if (comp.KnownSpells.Count > 3)
	{
		_popup.PopupEntity(Loc.GetString("cult-spell-exceeded"), uid, uid);
		return;
	}

	// If not standing on empowering rune and has spells, forget all of them to make room for the new one
	if (!standingOnRune && comp.KnownSpells.Count > 0)
	{
		// Remove all actions for known spells
		for (int i = comp.KnownSpells.Count - 1; i >= 0; i--)
		{
			var knownSpellId = comp.KnownSpells[i];
			var knownSpell = GetSpell(knownSpellId);
			if (knownSpell.ActionPrototypes != null)
			{
				foreach (var actionProto in knownSpell.ActionPrototypes)
				{
					// Find and remove all actions matching this prototype
					foreach (var action in _action.GetActions(uid))
					{
						var protoId = MetaData(action.Id).EntityPrototype?.ID;
						if (protoId != null && protoId == actionProto)
						{
							_action.RemoveAction(uid, action.Id);
						}
					}
				}
			}
		}
		// Clear the known spells list
		comp.KnownSpells.Clear();
	}

        if (data.Event != null)
            RaiseLocalEvent(uid, (object) data.Event, true);

		if (data.ActionPrototypes == null || data.ActionPrototypes.Count <= 0)
			return;

		if (data.DoAfterLength > 0)
		{
			_popup.PopupEntity(standingOnRune ? Loc.GetString("cult-spell-carving-rune") : Loc.GetString("cult-spell-carving"), uid, uid, PopupType.MediumCaution);
			var dargs = new DoAfterArgs(EntityManager, uid, data.DoAfterLength * (standingOnRune ? 1 : 3), new CarveSpellDoAfterEvent(
				uid, data, recordKnownSpell, standingOnRune), uid
			)
			{
				BreakOnDamage = true,
				RequireCanInteract = false,  // Allow restrained cultists to prepare spells
				NeedHand = false,  // Cultists don't need hands to prep spells
				BreakOnHandChange = false,
				BreakOnMove = true,
				BreakOnDropItem = false,
				CancelDuplicate = false,
			};

			_doAfter.TryStartDoAfter(dargs);
		}
		else
		{
			foreach (var act in data.ActionPrototypes)
				_action.AddAction(uid, act);
			if (recordKnownSpell)
				comp.KnownSpells.Add(data);
		}
	}

	public void OnCarveSpellDoAfter(Entity<BloodCultistComponent> ent, ref CarveSpellDoAfterEvent args)
	{
		if (ent.Comp.KnownSpells.Count > 3)
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-exceeded"), ent, ent);
			return;
		}

	// If not standing on empowering rune and has spells, forget all of them to make room for the new one
	if (!args.StandingOnRune && ent.Comp.KnownSpells.Count > 0)
	{
		// Remove all actions for known spells
		for (int i = ent.Comp.KnownSpells.Count - 1; i >= 0; i--)
		{
			var knownSpellId = ent.Comp.KnownSpells[i];
			var knownSpell = GetSpell(knownSpellId);
			if (knownSpell.ActionPrototypes != null)
			{
			foreach (var actionProto in knownSpell.ActionPrototypes)
			{
				// Find and remove all actions matching this prototype
				foreach (var action in _action.GetActions(ent))
				{
					var protoId = MetaData(action.Id).EntityPrototype?.ID;
					if (protoId != null && protoId == actionProto)
					{
						_action.RemoveAction(ent, action.Id);
					}
				}
			}
			}
		}
		// Clear the known spells list
		ent.Comp.KnownSpells.Clear();
	}

		if (args.CultAbility.ActionPrototypes == null)
			return;

		DamageSpecifier appliedDamageSpecifier = new DamageSpecifier(
			_protoMan.Index<DamageTypePrototype>("Slash"),
			FixedPoint2.New(args.CultAbility.HealthDrain * (args.StandingOnRune ? 1 : 3))
		);

        if (!args.Cancelled)
		{
			foreach (var act in args.CultAbility.ActionPrototypes)
			{
				_action.AddAction(args.CarverUid, act);
			}
			if (args.RecordKnownSpell)
				ent.Comp.KnownSpells.Add(args.CultAbility);
			
			_damageableSystem.TryChangeDamage(ent, appliedDamageSpecifier, true, origin: ent);
			_audioSystem.PlayPvs(args.CultAbility.CarveSound, ent);
			if (args.StandingOnRune)
				_bloodCultRules.Speak(ent, Loc.GetString("cult-invocation-empowering"));
		}

        Dirty(ent, ent.Comp);
	}

	public void RemoveSpell(ProtoId<CultAbilityPrototype> id, BloodCultistComponent comp)
	{
		comp.KnownSpells.Remove(GetSpell(id));
	}

	private void OnStudyVeil(Entity<BloodCultistComponent> ent, ref EventCultistStudyVeil args)
	{
		if (!TryUseAbility(ent, args))
			return;

		ent.Comp.StudyingVeil = true;
		args.Handled = true;
	}

	private void OnCommune(Entity<BloodCultistComponent> ent, ref BloodCultCommuneSendMessage args)
	{
		ent.Comp.CommuningMessage = args.Message;
	}

	private void OnSpellSelectedMessage(Entity<BloodCultistComponent> ent, ref SpellsMessage args)
	{
		if (!CultistSpellComponent.ValidSpells.Contains(args.ProtoId) || ent.Comp.KnownSpells.Contains(args.ProtoId))
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-havealready"), ent, ent);
			return;
		}
		AddSpell(ent, ent.Comp, args.ProtoId, recordKnownSpell:true);
	}

	private void OnSummonDagger(Entity<BloodCultistComponent> ent, ref EventCultistSummonDagger args)
	{
		if (!TryUseAbility(ent, args))
			return;

		var dagger = Spawn(_random.Pick(AvailableDaggers), Transform(ent).Coordinates);
		if (!_hands.TryForcePickupAnyHand(ent, dagger))
		{
			_popup.PopupEntity(Loc.GetString("cult-spell-fail"), ent, ent);
			QueueDel(dagger);
			return;
		}

		args.Handled = true;
	}

	private void OnStun(Entity<BloodCultistComponent> ent, ref EventCultistStun args)
	{
		if (args.Handled)
			return;

		// Check if targeting a teammate before consuming the spell
		if (HasComp<BloodCultistComponent>(args.Target) || HasComp<BloodCultConstructComponent>(args.Target))
			return;

		if (!TryUseAbility(ent, args))
			return;

		float staminaDamage = 90f;
		float empDamage = 1000f;
		int stunTime = 10;
		int selfStunTime = 4;

		args.Handled = true;

		var target = args.Target;

		if (HasComp<CultResistantComponent>(target))
		{
			_popup.PopupEntity(
					Loc.GetString("cult-spell-repelled"),
					ent, ent, PopupType.MediumCaution
				);
			_audioSystem.PlayPvs("/Audio/Effects/holy.ogg", Transform(ent).Coordinates);
			_stun.TryKnockdown(ent, TimeSpan.FromSeconds(selfStunTime), true);
		}
		else if (HasComp<BorgChassisComponent>(target) &&
			_powerCell.TryGetBatteryFromSlot(target, out EntityUid? batteryUid, out BatteryComponent? _) &&
			batteryUid != null)
		{
			_emp.DoEmpEffects((EntityUid)batteryUid, empDamage, stunTime);
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(stunTime), false);
		}
		else
		{
			_stun.TryKnockdown(target, TimeSpan.FromSeconds(stunTime), true);
			_stamina.TakeStaminaDamage(target, staminaDamage, visual: false);
			EnsureComp<CultMarkedComponent>(target);
			_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(stunTime), false);
		}
	}

	private void OnMarkedAttacked(Entity<CultMarkedComponent> ent, ref AttackedEvent args)
	{
		var advancedStaminaDamage = 100;
		var advancedStunTime = 15;
		if (HasComp<BloodCultRuneCarverComponent>(args.Used))
		{
			_stun.TryKnockdown(ent, TimeSpan.FromSeconds(advancedStunTime), true);
			_stamina.TakeStaminaDamage(ent, advancedStaminaDamage, visual: false);
			_stun.TryStun(ent, TimeSpan.FromSeconds(advancedStunTime), true);
			_statusEffect.TryAddStatusEffect<MutedComponent>(ent, "Muted", TimeSpan.FromSeconds(advancedStunTime), false);
			_entMan.RemoveComponent<CultMarkedComponent>(ent);
			_audioSystem.PlayPvs(new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg"), ent, AudioParams.Default.WithVolume(-3f));
		}
	}

	private void OnTwistedConstruction(Entity<BloodCultistComponent> ent, ref EventCultistTwistedConstruction args)
	{
		var canConvert = TryComp<StackComponent>(args.Target, out var stack) && (stack.StackTypeId == "Plasteel");
		if (stack == null || !canConvert || !TryUseAbility(ent, args))
			return;
		var count = stack.Count;
		var thirties_to_spawn = (int)((float)count / 30.0f);
		var tens_to_spawn = (int)(((float)count - 30.0f*(float)thirties_to_spawn) / 10.0f);
		var ones_to_spawn = (int)(((float)count - 10.0f*(float)tens_to_spawn) - 30.0f*(float)thirties_to_spawn);

		for (int i = 0; i < thirties_to_spawn; i++)
			Spawn("SheetRunedMetal30", Transform(args.Target).Coordinates);
		for (int i = 0; i < tens_to_spawn; i++)
			Spawn("SheetRunedMetal10", Transform(args.Target).Coordinates);
		for (int i = 0; i < ones_to_spawn; i++)
			Spawn("SheetRunedMetal1", Transform(args.Target).Coordinates);

		QueueDel(args.Target);
		args.Handled = true;
	}
}
