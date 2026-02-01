// SPDX-FileCopyrightText: 2025 MaiaArai <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 Zergologist <114537969+Chedd-Error@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.CCVar;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.IdentityManagement;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Verbs;
using Content.Shared.Interaction;
using Content.Shared.Atmos.Rotting;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Random;
using Robust.Shared.Configuration;
using Content.Shared.Traits.Assorted;
using Content.Shared._Shitmed.Targeting;
// Shitmed forced me to add this to apply damage to only the torso
using Content.Shared.Damage.Prototypes;

namespace Content.Shared.Cpr;

/// <summary>
/// Used for handling CPR on critical breathing mobs
/// </summary>
public abstract partial class SharedCprSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedInteractionSystem _interactionSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly EntityManager Ent = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MobThresholdSystem _mobThreshold = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedRottingSystem _rotting = default!;

    public const float CprInteractionRangeMultiplier = 0.25f;
    public const float CprDoAfterDelay = 0.5f;
    public const float CprAnimationLength = 0.2f;
    public const float CprAnimationEndTime = 1f;
    public const float CprManualEffectDuration = 5f;
    public const float CprManualThreshold = 1.5f;
    public const float CprReviveChance = 0.05f;

    public const float CprAirlossHealThreshold = 190f; // The damage threshold above which CPR will start healing airloss on a dead patient
    public const float CprAirlossHealAmount = 1f; // The amount of airloss to heal per CPR pump
    public const string AirlossDamageType = "Asphyxiation";

    private bool _cprRepeat;

    public override void Initialize()
    {
        base.Initialize();

        _config.OnValueChanged(CCVars.CprRepeat, value => _cprRepeat = value, true);

        SubscribeLocalEvent<CprComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAlternativeVerbs);
        SubscribeLocalEvent<CprComponent, CprDoAfterEvent>(OnCprDoAfter);

        SubscribeLocalEvent<CprComponent, GetInteractingEntitiesEvent>(OnGetInteractingEntities);
    }

    private void OnGetInteractingEntities(Entity<CprComponent> ent, ref GetInteractingEntitiesEvent args)
    {
        if (ent.Comp.LastCaretaker is { } user && !CprCaretakerOutdated(ent.Comp))
        {
            args.InteractingEntities.Add(user);
        }
    }

    public bool CprCaretakerOutdated(CprComponent cpr)
    {
        return Timing.CurTime.Seconds - cpr.LastTimeGivenCare.Seconds > CprDoAfterDelay;
    }

    public bool CanDoCpr(EntityUid recipient, EntityUid giver)
    {
        if (!HasComp<CprComponent>(recipient))
            return false;

        if (!_mobState.IsIncapacitated(recipient))
            return false;

        if (_mobState.IsIncapacitated(giver))
            return false;

        return true;
    }

    public bool InRangeForCpr(EntityUid recipient, EntityUid giver)
    {
        return _interactionSystem.InRangeUnobstructed(giver, recipient, SharedInteractionSystem.InteractionRange * CprInteractionRangeMultiplier);
    }

    public void OnCprDoAfter(Entity<CprComponent> ent, ref CprDoAfterEvent args)
    {
        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            ent.Comp.LastCaretaker = null;
            ent.Comp.LastTimeGivenCare = TimeSpan.Zero;
            Dirty(ent, ent.Comp);
            return;
        }

        if (!CanDoCpr(ent, args.User))
            return;

        if (!TryComp<DamageableComponent>(ent, out var damage) ||
            !TryComp<CprComponent>(ent, out var cpr) ||
            !TryComp<MobStateComponent>(ent, out var mobState))
            return;

        DoLunge(args.User);

        _audio.PlayPredicted(cpr.Sound, ent.Owner, args.User);

        // if the patient is dead, roll for a revive chance
        if (_mobState.IsDead(ent.Owner, mobState))
        {
            // heal airloss if the patient is dead and has high total damage
            if (damage.TotalDamage > CprAirlossHealThreshold)
            {
                var airlossHeal = new DamageSpecifier();
                airlossHeal.DamageDict.Add(AirlossDamageType, -CprAirlossHealAmount);

                _damage.TryChangeDamage(ent.Owner, airlossHeal, interruptsDoAfters: false, ignoreResistances: true, damageable: damage);
            }

            // try to get the dead threshold, if it's missing, we just proceed anyways
            bool hasDeadThreshold = _mobThreshold.TryGetThresholdForState(ent.Owner, MobState.Dead, out var threshold);
            bool isHealedEnough = !hasDeadThreshold || damage.TotalDamage < threshold;

            // only revive if they don't have the revivable component, aren't rotting, are above 200 damage, and have rolled lucky on the revive chance
            if (!HasComp<UnrevivableComponent>(ent) &&
                !_rotting.IsRotten(ent) &&
                isHealedEnough &&
                _random.Prob(CprReviveChance))
            {
                // determine the state to revive into based on current damage
                // defaults to alive
                var targetState = MobState.Alive;

                // only go to critical if they actually have enough damage to be critical
                if (_mobThreshold.TryGetThresholdForState(ent.Owner, MobState.Critical, out var critThreshold) &&
                    damage.TotalDamage > critThreshold)
                {
                    targetState = MobState.Critical;
                }

                _mobState.ChangeMobState(ent.Owner, targetState, mobState);

                if (_mobState.IsCritical(ent.Owner, mobState) || _mobState.IsAlive(ent.Owner, mobState))
                {
                    _popup.PopupPredicted(Loc.GetString("cpr-revive-success", ("target", ent.Owner)), args.User, args.User);
                }
            }
        }

        // Applies brute damage
        var scaledDamage = _cprRepeat
            ? cpr.Change
            : cpr.Change * ((CprManualEffectDuration - CprManualThreshold) / CprDoAfterDelay);

            _damage.TryChangeDamage(ent.Owner, scaledDamage, interruptsDoAfters: false, ignoreResistances: true, damageable: damage, targetPart: TargetBodyPart.Torso);
        var assist = EnsureComp<AssistedRespirationComponent>(ent);

        var newUntil = _cprRepeat
            ? Timing.CurTime + TimeSpan.FromSeconds(CprDoAfterDelay + 0.25f)
            :  Timing.CurTime + TimeSpan.FromSeconds(CprManualEffectDuration);

        if (newUntil > assist.AssistedUntil)
            assist.AssistedUntil = newUntil;

        // if they are NOT incapacitated apply bonus healing
        if (!_mobState.IsIncapacitated(ent.Owner, mobState) && cpr.BonusHeal != null)
        {
            var healing = new DamageSpecifier(cpr.BonusHeal);
            healing.DamageDict.Remove("Bloodloss");

            _damage.TryChangeDamage(ent.Owner, healing, interruptsDoAfters: false, ignoreResistances: true, damageable: damage);
        }

        cpr.LastCaretaker = args.User;
        cpr.LastTimeGivenCare = Timing.CurTime;
        Dirty(ent, cpr);

        // repeat if the mob is still in any crit state or dead
        args.Repeat = _mobState.IsIncapacitated(ent.Owner, mobState) && _cprRepeat;
        args.Handled = true;
    }

    public abstract void DoLunge(EntityUid user);

    public void TryStartCpr(EntityUid recipient, EntityUid giver)
    {
        if (!TryComp<CprComponent>(recipient, out var cpr))
            return;

        if (!CanDoCpr(recipient, giver) || !InRangeForCpr(recipient, giver))
            return;

        // Check if someone else is already performing a DoAfter on this target
        var interactingEntities = new HashSet<EntityUid>();
        _interactionSystem.GetEntitiesInteractingWithTarget(recipient, interactingEntities);

        // Remove self from the set (we're allowed to start CPR on someone we're already doing CPR on)
        interactingEntities.Remove(giver);

        if (interactingEntities.Count > 0)
        {
            _popup.PopupClient(Loc.GetString("cpr-already-in-progress"), recipient, giver);
            return;
        }

        // Set LastCaretaker immediately for client-side prediction and interaction tracking
        cpr.LastCaretaker = giver;
        cpr.LastTimeGivenCare = Timing.CurTime;
        Dirty(recipient, cpr);

        var doAfterEventArgs = new DoAfterArgs(
            EntityManager,
            giver,
            TimeSpan.FromSeconds(CprDoAfterDelay),
            new CprDoAfterEvent(),
            recipient,
            recipient
            )
        {
            BreakOnMove = true,
            DistanceThreshold = SharedInteractionSystem.InteractionRange * CprInteractionRangeMultiplier,
            BlockDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameTarget,
            RequireCanInteract = true,
            NeedHand = true
        };

        if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
        {
            // DoAfter failed to start - reset LastCaretaker
            cpr.LastCaretaker = null;
            cpr.LastTimeGivenCare = TimeSpan.Zero;
            Dirty(recipient, cpr);
            return;
        }

        var timeLeft = TimeSpan.Zero;
        if (TryComp<AssistedRespirationComponent>(recipient, out var comp))
            timeLeft = comp.AssistedUntil - Timing.CurTime;

        var recommendedRate = Math.Round(CprManualEffectDuration - CprManualThreshold);
        if (comp is null)
        {
            var localString = Loc.GetString("cpr-start-you", ("target", Identity.Entity(recipient, EntityManager)));
            var othersString = Loc.GetString("cpr-start", ("person", Identity.Entity(giver, EntityManager)), ("target", Identity.Entity(recipient, EntityManager)));
            _popup.PopupPredicted(localString, othersString, giver, giver, PopupType.Medium);
        }
        else if (!_cprRepeat && timeLeft <= TimeSpan.Zero)
        {
            _popup.PopupCursor(Loc.GetString("cpr-too-slow", ("seconds", recommendedRate)), giver, PopupType.Large);
        }
        else if (timeLeft > TimeSpan.FromSeconds(CprManualEffectDuration - CprManualThreshold))
        {
            _popup.PopupCursor(Loc.GetString("cpr-too-fast", ("seconds", recommendedRate)), giver, PopupType.Large);
        }
    }

    private void OnGetAlternativeVerbs(EntityUid uid, CprComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!CanDoCpr(uid, args.User))
            return;

        var inRange = InRangeForCpr(uid, args.User);

        var verb = new AlternativeVerb()
        {
            Act = () =>
            {
                TryStartCpr(uid, args.User);
            },
            Text = Loc.GetString("cpr-verb-text"),
            Priority = 5,
            Disabled = !inRange,
            Message = inRange ? null : Loc.GetString("cpr-verb-text-disabled"),
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/cpr.svg.192dpi.png"))
        };

        args.Verbs.Add(verb);
    }
}

[Serializable, NetSerializable]
public sealed partial class CprDoAfterEvent : DoAfterEvent
{
    public override DoAfterEvent Clone() => this;
}
