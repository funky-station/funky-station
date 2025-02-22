using Robust.Shared.Random;
using Robust.Shared.Prototypes;
using Robust.Shared.Audio.Systems;
using Content.Shared.FixedPoint;
using Content.Server.Popups;
using Content.Server.Hands.Systems;
using Content.Shared.Actions;
using Content.Shared.BloodCult;
using Content.Shared.BloodCult.Prototypes;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Server.GameTicking.Rules;

namespace Content.Server.BloodCult.EntitySystems;

public sealed partial class CultistSpellSystem : EntitySystem
{
	[Dependency] private readonly IRobustRandom _random = default!;
	[Dependency] private readonly IPrototypeManager _proto = default!;
	[Dependency] private readonly SharedActionsSystem _action = default!;
	[Dependency] private readonly DamageableSystem _damageableSystem = default!;
	[Dependency] private readonly PopupSystem _popup = default!;
	[Dependency] private readonly SharedAudioSystem _audioSystem = default!;
	[Dependency] private readonly BloodCultRuleSystem _bloodCultRules = default!;
	[Dependency] private readonly HandsSystem _hands = default!;
	[Dependency] private readonly IPrototypeManager _protoMan = default!;

	private static string[] AvailableDaggers = ["CultDaggerCurved", "CultDaggerSerrated", "CultDaggerStraight"];

	public override void Initialize()
	{
		base.Initialize();

		SubscribeLocalEvent<BloodCultistComponent, EventCultistSummonDagger>(OnSummonDagger);
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
}
