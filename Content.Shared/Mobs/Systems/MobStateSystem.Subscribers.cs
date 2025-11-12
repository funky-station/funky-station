// SPDX-FileCopyrightText: 2023 DrSmugleaf <drsmugleaf@gmail.com>
// SPDX-FileCopyrightText: 2023 Jezithyr <jezithyr@gmail.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Cojoke <83733158+Cojoke-dot@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Mr. 27 <45323883+Dutch-VanDerLinde@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Ilya Mikheev <me@ilyamikcoder.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Bed.Sleep;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.ForceSay;
using Content.Shared.Emoting;
using Content.Shared.Hands;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Pointing;
using Content.Shared.Pulling.Events;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.Tag;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Strip.Components;
using Content.Shared.Throwing;
using Robust.Shared.Prototypes;

namespace Content.Shared.Mobs.Systems;

public partial class MobStateSystem
{
    [Dependency] private readonly TagSystem _tag = default!;

    private static readonly ProtoId<TagPrototype> ForceStandOnReviveTag = "ForceStandOnRevive";

    //General purpose event subscriptions. If you can avoid it register these events inside their own systems
    private void SubscribeEvents()
    {
        SubscribeLocalEvent<MobStateComponent, BeforeGettingStrippedEvent>(OnGettingStripped);
        SubscribeLocalEvent<MobStateComponent, ChangeDirectionAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, UseAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, AttackAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, ConsciousAttemptEvent>(CheckConcious);
        SubscribeLocalEvent<MobStateComponent, ThrowAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, SpeakAttemptEvent>(OnSpeakAttempt);
        SubscribeLocalEvent<MobStateComponent, IsEquippingAttemptEvent>(OnEquipAttempt);
        SubscribeLocalEvent<MobStateComponent, EmoteAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, IsUnequippingAttemptEvent>(OnUnequipAttempt);
        SubscribeLocalEvent<MobStateComponent, DropAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, PickupAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, StartPullAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, UpdateCanMoveEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, StandAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, PointAttemptEvent>(CheckAct);
        SubscribeLocalEvent<MobStateComponent, TryingToSleepEvent>(OnSleepAttempt);
        SubscribeLocalEvent<MobStateComponent, CombatModeShouldHandInteractEvent>(OnCombatModeShouldHandInteract);
        SubscribeLocalEvent<MobStateComponent, AttemptPacifiedAttackEvent>(OnAttemptPacifiedAttack);
        SubscribeLocalEvent<MobStateComponent, DamageModifyEvent>(OnDamageModify);

        SubscribeLocalEvent<MobStateComponent, UnbuckleAttemptEvent>(OnUnbuckleAttempt);
    }

    private void OnUnbuckleAttempt(Entity<MobStateComponent> ent, ref UnbuckleAttemptEvent args)
    {
        // TODO is this necessary?
        // Shouldn't the interaction have already been blocked by a general interaction check?
        if (args.User == ent.Owner && IsIncapacitated(ent))
            args.Cancelled = true;
    }

    private void CheckConcious(Entity<MobStateComponent> ent, ref ConsciousAttemptEvent args)
    {
        switch (ent.Comp.CurrentState)
        {
            case MobState.Dead:
            case MobState.Critical:
                args.Cancelled = true;
                break;
        }
    }

    private void OnStateExitSubscribers(EntityUid target, MobStateComponent component, MobState state)
    {
        switch (state)
        {
            case MobState.Alive:
                //unused
                break;
            case MobState.Critical:
                var forceStand = false;
                if (TryComp<TagComponent>(target, out var tag))
                    forceStand = _tag.HasTag(tag, ForceStandOnReviveTag);

                _standing.Stand(target, force: forceStand);
                break;
            case MobState.Dead:
                RemComp<CollisionWakeComponent>(target);
                _standing.Stand(target);
                break;
            case MobState.Invalid:
                //unused
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private void OnStateEnteredSubscribers(EntityUid target, MobStateComponent component, MobState state)
    {
        // All of the state changes here should already be networked, so we do nothing if we are currently applying a
        // server state.
        if (_timing.ApplyingState)
            return;

        _blocker.UpdateCanMove(target); //update movement anytime a state changes
        switch (state)
        {
            case MobState.Alive:
                if (HasComp<BorgChassisComponent>(target))
                    _standing.Stand(target, force: true);
                else
                    _standing.Stand(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Alive);
                break;
            case MobState.Critical:
                _standing.Down(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Critical);
                break;
            case MobState.Dead:
                EnsureComp<CollisionWakeComponent>(target);
                _standing.Down(target);
                _appearance.SetData(target, MobStateVisuals.State, MobState.Dead);
                break;
            case MobState.Invalid:
                //unused;
                break;
            default:
                throw new NotImplementedException();
        }
    }

    #region Event Subscribers

    private void OnSleepAttempt(EntityUid target, MobStateComponent component, ref TryingToSleepEvent args)
    {
        if (IsDead(target, component))
            args.Cancelled = true;
    }

    private void OnGettingStripped(EntityUid target, MobStateComponent component, BeforeGettingStrippedEvent args)
    {
        // Incapacitated or dead targets get stripped two or three times as fast. Makes stripping corpses less tedious.
        if (IsDead(target, component))
            args.Multiplier /= 3;
        else if (IsCritical(target, component))
            args.Multiplier /= 2;
    }

    private void OnSpeakAttempt(EntityUid uid, MobStateComponent component, SpeakAttemptEvent args)
    {
        if (HasComp<AllowNextCritSpeechComponent>(uid))
        {
            RemCompDeferred<AllowNextCritSpeechComponent>(uid);
            return;
        }

        CheckAct(uid, component, args);
    }

    private void CheckAct(EntityUid target, MobStateComponent component, CancellableEntityEventArgs args)
    {
        switch (component.CurrentState)
        {
            case MobState.Dead:
            case MobState.Critical:
                args.Cancel();
                break;
        }
    }

    private void OnEquipAttempt(EntityUid target, MobStateComponent component, IsEquippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (args.Equipee == target)
            CheckAct(target, component, args);
    }

    private void OnUnequipAttempt(EntityUid target, MobStateComponent component, IsUnequippingAttemptEvent args)
    {
        // is this a self-equip, or are they being stripped?
        if (args.Unequipee == target)
            CheckAct(target, component, args);
    }

    private void OnCombatModeShouldHandInteract(EntityUid uid, MobStateComponent component, ref CombatModeShouldHandInteractEvent args)
    {
        // Disallow empty-hand-interacting in combat mode
        // for non-dead mobs
        if (!IsDead(uid, component))
            args.Cancelled = true;
    }

    private void OnAttemptPacifiedAttack(Entity<MobStateComponent> ent, ref AttemptPacifiedAttackEvent args)
    {
        args.Cancelled = true;
    }

    private void OnDamageModify(Entity<MobStateComponent> ent, ref DamageModifyEvent args)
    {
        args.Damage *= _damageable.UniversalMobDamageModifier;
    }

    #endregion
}
