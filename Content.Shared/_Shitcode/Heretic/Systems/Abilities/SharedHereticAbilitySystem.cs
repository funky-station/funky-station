using System.Linq;
using Content.Shared._Shitcode.Heretic.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Actions;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Heretic;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Projectiles;
using Content.Shared.Standing;
using Content.Shared.StatusEffect;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Shitcode.Heretic.Systems.Abilities;

public abstract partial class SharedHereticAbilitySystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] protected readonly SharedDoAfterSystem DoAfter = default!;
    [Dependency] protected readonly EntityLookupSystem Lookup = default!;
    [Dependency] protected readonly StatusEffectsSystem Status = default!;
    [Dependency] private readonly StatusEffectNew.StatusEffectsSystem _statusNew = default!;
    [Dependency] private readonly SharedProjectileSystem _projectile = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throw = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedEyeSystem _eye = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    [Dependency] protected readonly SharedPopupSystem Popup = default!;

    public static readonly DamageSpecifier AllDamage = new()
    {
        DamageDict =
        {
            {"Blunt", 1},
            {"Slash", 1},
            {"Piercing", 1},
            {"Heat", 1},
            {"Cold", 1},
            {"Shock", 1},
            {"Asphyxiation", 1},
            {"Bloodloss", 1},
            {"Caustic", 1},
            {"Poison", 1},
            {"Radiation", 1},
            {"Cellular", 1},
            {"Holy", 1},
        },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAsh();
        SubscribeFlesh();
        SubscribeSide();

    }

    protected List<Entity<MobStateComponent>> GetNearbyPeople(EntityUid ent,
        float range,
        string? path,
        EntityCoordinates? coords = null)
    {
        var list = new List<Entity<MobStateComponent>>();
        var lookup = Lookup.GetEntitiesInRange<MobStateComponent>(coords ?? Transform(ent).Coordinates, range);

        foreach (var look in lookup)
        {
            // ignore heretics with the same path*, affect everyone else
            if (TryComp<HereticComponent>(look, out var th) && th.CurrentPath == path || HasComp<GhoulComponent>(look))
                continue;

            if (!HasComp<StatusEffectsComponent>(look))
                continue;

            list.Add(look);
        }

        return list;
    }

    public bool TryUseAbility(EntityUid ent, BaseActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!TryComp<HereticActionComponent>(args.Action, out var actionComp))
            return false;

        // check if any magic items are worn
        if (!TryComp<HereticComponent>(ent, out var hereticComp) || !actionComp.RequireMagicItem ||
            hereticComp.Ascended)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        var ev = new CheckMagicItemEvent();
        RaiseLocalEvent(ent, ev);

        if (ev.Handled)
        {
            SpeakAbility(ent, actionComp);
            return true;
        }

        // Almost all of the abilites are serverside anyway
        if (_net.IsServer)
            Popup.PopupEntity(Loc.GetString("heretic-ability-fail-magicitem"), ent, ent);

        return false;
    }

    private EntityUid? GetTouchSpell<TEvent, TComp>(Entity<HereticComponent> ent, ref TEvent args)
        where TEvent : InstantActionEvent, ITouchSpellEvent
        where TComp : Component
    {
        if (!TryUseAbility(ent, args))
            return null;

        if (!TryComp(ent, out HandsComponent? hands) || hands.Hands.Count < 1)
            return null;

        args.Handled = true;

        var hasComp = false;

        foreach (var held in _hands.EnumerateHeld((ent, hands)))
        {
            if (!HasComp<TComp>(held))
                continue;

            hasComp = true;
            PredictedQueueDel(held);
        }

        if (hasComp || !_hands.TryGetEmptyHand((ent, hands), out var emptyHand))
            return null;

        var touch = PredictedSpawnAtPosition(args.TouchSpell, Transform(ent).Coordinates);

        if (_hands.TryPickup(ent, touch, emptyHand, animate: false, handsComp: hands))
            return touch;

        PredictedQueueDel(touch);
        return null;
    }

    /// <summary>
    /// Heals everything imaginable
    /// </summary>
    /// <param name="uid">Entity to heal</param>
    /// <param name="toHeal">how much to heal, null = full heal</param>
    /// <param name="bloodHeal">how much to restore blood, null = fully restore</param>
    /// <param name="bleedHeal">how much to heal bleeding, null = full heal</param>
    public void IHateWoundMed(Entity<DamageableComponent?, BodyComponent?> uid,
        DamageSpecifier? toHeal,
        FixedPoint2? bloodHeal,
        FixedPoint2? bleedHeal)
    {
        if (!Resolve(uid, ref uid.Comp1, false))
            return;

        if (toHeal != null)
        {
            _dmg.TryChangeDamage(uid,
                toHeal,
                true,
                false,
                uid.Comp1,
                targetPart: TargetBodyPart.All);
        }
        else
        {
            TryComp<MobThresholdsComponent>(uid, out var thresholds);
            // do this so that the state changes when we set the damage
            _mobThreshold.SetAllowRevives(uid, true, thresholds);
            _dmg.SetAllDamage(uid, uid.Comp1, 0);
            _mobThreshold.SetAllowRevives(uid, false, thresholds);
        }

        if (bleedHeal == FixedPoint2.Zero && bloodHeal == FixedPoint2.Zero ||
            !TryComp(uid, out BloodstreamComponent? blood))
            return;

        if (bleedHeal != FixedPoint2.Zero && blood.BleedAmount > 0f)
        {
            if (bleedHeal == null)
                _blood.TryModifyBleedAmount((uid, blood), -blood.BleedAmount);
            else
                _blood.TryModifyBleedAmount((uid, blood), bleedHeal.Value.Float());
        }

        if (bloodHeal == FixedPoint2.Zero || !TryComp(uid, out SolutionContainerManagerComponent? sol) ||
            !_solution.ResolveSolution((uid, sol), blood.BloodSolutionName, ref blood.BloodSolution) ||
            blood.BloodSolution.Value.Comp.Solution.Volume >= blood.MaxVolumeModifier)
            return;

        if (bloodHeal == null)
        {
            _blood.TryModifyBloodLevel((uid, blood),
                blood.MaxVolumeModifier - blood.BloodSolution.Value.Comp.Solution.Volume);
        }
        else
        {
            _blood.TryModifyBloodLevel((uid, blood),
                FixedPoint2.Min(bloodHeal.Value,
                    blood.MaxVolumeModifier - blood.BloodSolution.Value.Comp.Solution.Volume));
        }
    }

    public virtual void InvokeTouchSpell<T>(Entity<T> ent, EntityUid user) where T : Component, ITouchSpell
    {
        _audio.PlayPredicted(ent.Comp.Sound, user, user);
    }
    protected virtual void SpeakAbility(EntityUid ent, HereticActionComponent args) { }
}
