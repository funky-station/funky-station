// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
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
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using Robust.Shared.Random;
using Robust.Shared.Configuration;
using Content.Shared.Traits.Assorted;

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

    public const float CprInteractionRangeMultiplier = 0.25f;
    public const float CprDoAfterDelay = 0.7f;
    public const float CprAnimationLength = 0.2f;
    public const float CprAnimationEndTime = 1f;
    public const float CprManualEffectDuration = 5f;
    public const float CprManualThreshold = 1.5f;
    public const float CprReviveChance = 0.05f;

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
        if (!TryComp<CprComponent>(recipient, out var cpr) ||
            !TryComp<MobThresholdsComponent>(recipient, out var thresholds) ||
            !TryComp<MobThresholdsComponent>(giver, out var myThresholds))
            return false;

        if ((thresholds.CurrentThresholdState != MobState.Critical && thresholds.CurrentThresholdState != MobState.Dead) ||
            myThresholds.CurrentThresholdState == MobState.Critical ||
            myThresholds.CurrentThresholdState == MobState.Dead)
            return false;

        if (cpr.LastCaretaker.HasValue &&
            !CprCaretakerOutdated(cpr) &&
            cpr.LastCaretaker.Value != giver)
            return false;

        return true;
    }

    public bool InRangeForCpr(EntityUid recipient, EntityUid giver)
    {
        return _interactionSystem.InRangeUnobstructed(giver, recipient, SharedInteractionSystem.InteractionRange * CprInteractionRangeMultiplier);
    }

    public void OnCprDoAfter(Entity<CprComponent> ent, ref CprDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!CanDoCpr(ent, args.User))
            return;

        if (!TryComp<DamageableComponent>(ent, out var damage) ||
            !TryComp<CprComponent>(ent, out var cpr) ||
            !TryComp<MobThresholdsComponent>(ent, out var thresholds) ||
            !TryComp<MobStateComponent>(ent, out var mobState))
            return;

        DoLunge(args.User);

        _audio.PlayPredicted(cpr.Sound, ent.Owner, args.User);

        // If the patient is Dead, roll for a revive chance
        {
            if (!HasComp<UnrevivableComponent>(ent) &&
                _mobThreshold.TryGetThresholdForState(ent.Owner, MobState.Dead, out var threshold) &&
                damage.TotalDamage < threshold &&
                _random.Prob(CprReviveChance))
            {
                _mobState.ChangeMobState(ent.Owner, MobState.Critical, mobState);

                if (mobState.CurrentState == MobState.Critical)
                {
                    _popup.PopupPredicted(Loc.GetString("cpr-revive-success", ("target", ent.Owner)), args.User, args.User);
                }
            }
        }

        // Applies brute damage
        var scaledDamage = _cprRepeat
            ? cpr.Change
            : cpr.Change * ((CprManualEffectDuration - CprManualThreshold) / CprDoAfterDelay);

        _damage.TryChangeDamage(ent.Owner, scaledDamage, interruptsDoAfters: false, ignoreResistances: true, damageable: damage);

        var assist = EnsureComp<AssistedRespirationComponent>(ent);

        var newUntil = _cprRepeat
            ? Timing.CurTime + TimeSpan.FromSeconds(CprDoAfterDelay + 0.25f)
            :  Timing.CurTime + TimeSpan.FromSeconds(CprManualEffectDuration);

        if (newUntil > assist.AssistedUntil)
            assist.AssistedUntil = newUntil;

        if (mobState.CurrentState != MobState.Critical &&
            mobState.CurrentState != MobState.Dead &&
            cpr.BonusHeal != null)
        {
            var healing = new DamageSpecifier(cpr.BonusHeal);
            healing.DamageDict.Remove("Bloodloss");

            _damage.TryChangeDamage(ent.Owner, healing, interruptsDoAfters: false, ignoreResistances: true, damageable: damage);
        }

        cpr.LastCaretaker = args.User;
        cpr.LastTimeGivenCare = Timing.CurTime;

        args.Repeat = (mobState.CurrentState == MobState.Critical || mobState.CurrentState == MobState.Dead) && _cprRepeat;
        args.Handled = true;
    }

    public abstract void DoLunge(EntityUid user);

    public void TryStartCpr(EntityUid recipient, EntityUid giver)
    {
        var doAfterEventArgs = new DoAfterArgs(
            EntityManager,
            giver,
            TimeSpan.FromSeconds(CprDoAfterDelay),
            new CprDoAfterEvent(),
            recipient,
            giver
            )
        {
            BreakOnMove = true,
            BlockDuplicate = true,
            RequireCanInteract = true,
            NeedHand = true
        };

        if (!CanDoCpr(recipient, giver)
            || !InRangeForCpr(recipient, giver)
            || !_doAfter.TryStartDoAfter(doAfterEventArgs))
            return;

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
