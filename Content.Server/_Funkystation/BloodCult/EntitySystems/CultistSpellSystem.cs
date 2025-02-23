using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;
using Content.Server.Power.Components;
using Content.Shared.FixedPoint;
using Content.Server.Popups;
using Content.Server.Hands.Systems;
using Content.Shared.Actions;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Prototypes;
using Content.Server.BloodCult.Components;
using Content.Shared.BloodCult.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.StatusEffect;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Content.Shared.Emp;
using Content.Server.Emp;
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
	[Dependency] private readonly IGameTiming _gameTiming = default!;
	[Dependency] private readonly IEntityManager _entMan = default!;
	[Dependency] private readonly SharedStunSystem _stun = default!;
	
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	private static string[] AvailableDaggers = ["CultDaggerCurved", "CultDaggerSerrated", "CultDaggerStraight"];

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<BloodCultistComponent, EventCultistSummonDagger>(OnSummonDagger);

		SubscribeLocalEvent<BloodCultistComponent, EventCultistStudyVeil>(OnStudyVeil);
		SubscribeLocalEvent<BloodCultistComponent, EventCultistStun>(OnStun);
		SubscribeLocalEvent<CultMarkedComponent, AttackedEvent>(OnMarkedAttacked);
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

	public void AddSpell(EntityUid uid, BloodCultistComponent comp, ProtoId<CultAbilityPrototype> id)
	{
		var data = GetSpell(id);

        if (data.Event != null)
            RaiseLocalEvent(uid, (object) data.Event, true);

        if (data.ActionPrototypes != null && data.ActionPrototypes.Count > 0)
            foreach (var act in data.ActionPrototypes)
                _action.AddAction(uid, act);
		comp.KnownSpells.Add(data);

        Dirty(uid, comp);
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
		if (args.Handled || !TryUseAbility(ent, args) || HasComp<BloodCultistComponent>(args.Target))
			return;

		float staminaDamage = 90f;
		float empDamage = 1000f;
		int stunTime = 10;

		args.Handled = true;

		var target = args.Target;

		if (HasComp<BorgChassisComponent>(target) &&
			_powerCell.TryGetBatteryFromSlot(target, out EntityUid? batteryUid, out BatteryComponent? _) &&
			batteryUid != null)
		{
			_emp.DoEmpEffects((EntityUid)batteryUid, empDamage, stunTime);
		}
		else
		{
			_stun.TryKnockdown(target, TimeSpan.FromSeconds(stunTime), true);
			_stamina.TakeStaminaDamage(target, staminaDamage, visual: false);
			EnsureComp<CultMarkedComponent>(target);
		}

		_statusEffect.TryAddStatusEffect<MutedComponent>(target, "Muted", TimeSpan.FromSeconds(stunTime), false);

		args.Handled = true;
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
}
