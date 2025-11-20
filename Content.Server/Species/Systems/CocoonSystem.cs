using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Hands.Components;
using Content.Shared.Humanoid;
using Content.Shared.Verbs;
using Content.Shared.Interaction.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Species.Arachnid;
using Content.Shared.Standing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Server.Species.Arachnid;

public sealed class CocoonSystem : SharedCocoonSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly HungerSystem _hunger = default!;
    [Dependency] private readonly PopupSystem _popups = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CocoonerComponent, WrapActionEvent>(OnWrapAction);
        SubscribeLocalEvent<CocoonerComponent, WrapDoAfterEvent>(OnWrapDoAfter);

        SubscribeLocalEvent<CocoonedComponent, ComponentStartup>(OnCocoonStartup);
        SubscribeLocalEvent<CocoonedComponent, ComponentShutdown>(OnCocoonShutdown);

        SubscribeLocalEvent<CocoonedComponent, GetVerbsEvent<InteractionVerb>>(OnGetInteractionVerb);
        SubscribeLocalEvent<CocoonedComponent, UnwrapDoAfterEvent>(OnUnwrapDoAfter);

        SubscribeLocalEvent<CocoonedComponent, StandAttemptEvent>(OnStandAttempt);
        SubscribeLocalEvent<CocoonedComponent, DamageModifyEvent>(OnDamageModify);
    }

    private void OnDamageModify(Entity<CocoonedComponent> ent, ref DamageModifyEvent args)
    {
        // Only absorb positive damage
        if (!args.OriginalDamage.AnyPositive())
            return;

        var originalTotalDamage = args.OriginalDamage.GetTotal().Float();
        if (originalTotalDamage <= 0)
            return;

        // Calculate 30% of the original damage to absorb
        var absorbedDamage = originalTotalDamage * ent.Comp.AbsorbPercentage;
        
        // Reduce the damage by 30% (victim only takes 70%)
        // Apply coefficient to all damage types that were originally present
        var reducePercentage = 1f - ent.Comp.AbsorbPercentage;
        var modifier = new DamageModifierSet();
        foreach (var key in args.OriginalDamage.DamageDict.Keys)
        {
            modifier.Coefficients.TryAdd(key, reducePercentage);
        }
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifier);

        // Accumulate the absorbed damage on the cocoon
        ent.Comp.AccumulatedDamage += absorbedDamage;
        Dirty(ent, ent.Comp);

        // Break the cocoon if it reaches max damage
        if (ent.Comp.AccumulatedDamage >= ent.Comp.MaxDamage)
        {
            RemCompDeferred<CocoonedComponent>(ent);
            _popups.PopupEntity(Loc.GetString("arachnid-cocoon-broken"), ent, ent, PopupType.LargeCaution);
        }
    }

    private void OnStandAttempt(Entity<CocoonedComponent> ent, ref StandAttemptEvent args)
    {
        args.Cancel();
    }

    private void OnWrapAction(EntityUid uid, CocoonerComponent component, ref WrapActionEvent args)
    {
        if (args.Handled)
            return;

        var user = args.Performer;
        var target = args.Target;

        if (target == user)
            return;

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popups.PopupEntity(Loc.GetString("arachnid-wrap-invalid-target"), user, user);
            return;
        }

        if (HasComp<CocoonedComponent>(target))
        {
            _popups.PopupEntity(Loc.GetString("arachnid-wrap-already"), user, user);
            return;
        }

        if (!_blocker.CanInteract(user, target))
            return;

        // Only require hands if the entity has hands (spiders don't have hands)
        var needHand = HasComp<HandsComponent>(user);

        var doAfter = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(component.WrapDuration), new WrapDoAfterEvent(), user, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = needHand,
            DistanceThreshold = 1.5f,
            CancelDuplicate = true,
            BlockDuplicate = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        var mapCoords = _transform.GetMapCoordinates(target);
        var filter = Filter.Empty().AddInRange(mapCoords, 10f);
        var entityCoords = _transform.ToCoordinates(mapCoords);
        _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_start.ogg"), filter, entityCoords, true);

        _popups.PopupEntity(Loc.GetString("arachnid-wrap-start-user", ("target", target)), user, user);
        _popups.PopupEntity(Loc.GetString("arachnid-wrap-start-target", ("user", user)), target, target, PopupType.LargeCaution);

        args.Handled = true;
    }

    private void OnWrapDoAfter(EntityUid uid, CocoonerComponent component, ref WrapDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        var performer = args.User;
        var target = args.Args.Target.Value;

        if (!HasComp<HumanoidAppearanceComponent>(target) || HasComp<CocoonedComponent>(target))
            return;

        if (!_blocker.CanInteract(performer, target))
            return;

        // Only consume hunger if the entity has a HungerComponent
        if (TryComp<Content.Shared.Nutrition.Components.HungerComponent>(performer, out var hunger))
        {
            _hunger.ModifyHunger(performer, -component.HungerCost);
        }

        var cocoon = EnsureComp<CocoonedComponent>(target);
        Dirty(target, cocoon);

        // Play ziptie sound for everyone within 10 meters
        var mapCoords = _transform.GetMapCoordinates(target);
        var filter = Filter.Empty().AddInRange(mapCoords, 10f);
        var entityCoords = _transform.ToCoordinates(mapCoords);
        _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_end.ogg"), filter, entityCoords, true);

        _popups.PopupEntity(Loc.GetString("arachnid-wrap-complete-user", ("target", target)), performer, performer);
        _popups.PopupEntity(Loc.GetString("arachnid-wrap-complete-target"), target, target, PopupType.LargeCaution);

        args.Handled = true;
    }

    private void OnUnwrapDoAfter(EntityUid uid, CocoonedComponent component, ref UnwrapDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target == null)
            return;

        // Play ziptie sound for everyone within 10 meters
        var mapCoords = _transform.GetMapCoordinates(uid);
        var filter = Filter.Empty().AddInRange(mapCoords, 10f);
        var entityCoords = _transform.ToCoordinates(mapCoords);
        _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_breakout.ogg"), filter, entityCoords, true);

        RemCompDeferred<CocoonedComponent>(uid);

        _popups.PopupEntity(Loc.GetString("arachnid-unwrap-user", ("target", uid)), args.User, args.User);
        _popups.PopupEntity(Loc.GetString("arachnid-unwrap-target", ("user", args.User)), uid, uid);
    }

    private void OnCocoonStartup(EntityUid uid, CocoonedComponent component, ComponentStartup args)
    {
        // Force prone
        if (HasComp<StandingStateComponent>(uid))
        {
            _standing.Down(uid);
        }

        if (!HasComp<BlockMovementComponent>(uid))
        {
            AddComp<BlockMovementComponent>(uid);
        }

        EnsureComp<MumbleAccentComponent>(uid);
    }

    private void OnCocoonShutdown(EntityUid uid, CocoonedComponent component, ComponentShutdown args)
    {
        if (HasComp<BlockMovementComponent>(uid))
            RemComp<BlockMovementComponent>(uid);

        if (HasComp<MumbleAccentComponent>(uid))
        {
            RemComp<MumbleAccentComponent>(uid);
        }
    }

    private void OnGetInteractionVerb(EntityUid uid, CocoonedComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        // Must be in range, must be able to interact
        if (!args.CanAccess)
            return;

        // Can't unwrap yourself
        if (args.User == uid)
        {
            var verb = new InteractionVerb
            {
                Text = Loc.GetString("arachnid-unwrap-verb"),
                Priority = 1,
                Act = () =>
                {
                    _popups.PopupEntity(Loc.GetString("arachnid-unwrap-self"), uid, uid);
                }
            };
            args.Verbs.Add(verb);
        }
        else
        {
            if (!args.CanInteract)
                return;

            // Unwrapping verb
            var unwrapVerb = new InteractionVerb
            {
                Text = Loc.GetString("arachnid-unwrap-verb", ("target", uid)),

                Priority = 10,

                Act = () =>
                {
                    if (!_blocker.CanInteract(args.User, uid))
                        return;

                    // Only require hands if the entity has hands
                    var needHand = HasComp<HandsComponent>(args.User);

                    var doAfter = new DoAfterArgs(
                        EntityManager,
                        args.User,
                        TimeSpan.FromSeconds(10.0f),
                        new UnwrapDoAfterEvent(),
                        uid,
                        uid)
                    {
                        BreakOnMove = true,
                        BreakOnDamage = true,
                        NeedHand = needHand,
                        DistanceThreshold = 1.5f,
                        CancelDuplicate = true,
                        BlockDuplicate = true,
                    };

                    if (!_doAfter.TryStartDoAfter(doAfter))
                        return;

                    var mapCoords = _transform.GetMapCoordinates(uid);
                    var filter = Filter.Empty().AddInRange(mapCoords, 10f);
                    var entityCoords = _transform.ToCoordinates(mapCoords);
                    _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_start.ogg"), filter, entityCoords, true);

                    _popups.PopupEntity(Loc.GetString("arachnid-unwrap-start-user", ("target", uid)), args.User, args.User);
                    _popups.PopupEntity(Loc.GetString("arachnid-unwrap-start-target", ("user", args.User)), uid, uid);
                }
            };
            args.Verbs.Add(unwrapVerb);
        }
    }
}
