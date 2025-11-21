using System.Linq;
using Content.Server.Popups;
using Content.Server.Speech.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Verbs;
using Content.Shared.Interaction.Components;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Popups;
using Content.Shared.Species.Arachnid;
using Content.Shared.Standing;
using Content.Shared.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Player;
using Robust.Shared.Utility;

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
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CocoonerComponent, WrapActionEvent>(OnWrapAction);
        SubscribeLocalEvent<CocoonerComponent, WrapDoAfterEvent>(OnWrapDoAfter);

        // Container-based cocoon system
        SubscribeLocalEvent<CocoonContainerComponent, ComponentStartup>(OnCocoonContainerStartup);
        SubscribeLocalEvent<CocoonContainerComponent, ComponentShutdown>(OnCocoonContainerShutdown);
        SubscribeLocalEvent<CocoonContainerComponent, DamageModifyEvent>(OnCocoonContainerDamage);
        SubscribeLocalEvent<CocoonContainerComponent, GetVerbsEvent<InteractionVerb>>(OnGetUnwrapVerb);
        SubscribeLocalEvent<CocoonContainerComponent, UnwrapDoAfterEvent>(OnUnwrapDoAfter);
    }

    private void OnCocoonContainerDamage(Entity<CocoonContainerComponent> ent, ref DamageModifyEvent args)
    {
        // Only absorb positive damage
        if (!args.OriginalDamage.AnyPositive())
            return;

        var originalTotalDamage = args.OriginalDamage.GetTotal().Float();
        if (originalTotalDamage <= 0)
            return;

        // Calculate percentage of the original damage to absorb
        var absorbedDamage = originalTotalDamage * ent.Comp.AbsorbPercentage;
        
        // Reduce the damage by the absorb percentage (victim only takes the remainder)
        // Apply coefficient to all damage types that were originally present
        var reducePercentage = 1f - ent.Comp.AbsorbPercentage;
        var modifier = new DamageModifierSet();
        foreach (var key in args.OriginalDamage.DamageDict.Keys)
        {
            modifier.Coefficients.TryAdd(key, reducePercentage);
        }
        args.Damage = DamageSpecifier.ApplyModifierSet(args.Damage, modifier);

        // Accumulate the absorbed damage on the cocoon container
        ent.Comp.AccumulatedDamage += absorbedDamage;
        Dirty(ent, ent.Comp);

        // Pass the reduced damage to the victim inside
        if (ent.Comp.Victim != null && Exists(ent.Comp.Victim.Value))
        {
            // Apply the reduced damage directly to the victim
            _damageable.TryChangeDamage(ent.Comp.Victim.Value, args.Damage, origin: args.Origin);
        }

        // The container itself takes minimal/no damage (we handle breaking via accumulated damage)
        // Set damage to zero so the container doesn't take structural damage
        args.Damage = new DamageSpecifier();

        // Break the cocoon if it reaches max damage
        if (ent.Comp.AccumulatedDamage >= ent.Comp.MaxDamage)
        {
            BreakCocoon(ent);
        }
    }

    /// <summary>
    ///     Plays the cocoon removal sound for everyone within range.
    /// </summary>
    private void PlayCocoonRemovalSound(EntityUid uid)
    {
        var mapCoords = _transform.GetMapCoordinates(uid);
        var filter = Filter.Empty().AddInRange(mapCoords, 10f);
        var entityCoords = _transform.ToCoordinates(mapCoords);
        _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_breakout.ogg"), filter, entityCoords, true);
    }

    private void OnCocoonContainerStartup(EntityUid uid, CocoonContainerComponent component, ComponentStartup args)
    {
        // Victim setup is handled after insertion in OnWrapDoAfter
        // This is because ComponentStartup may fire before the victim is set
    }

    /// <summary>
    /// Applies effects to a victim when they are cocooned.
    /// </summary>
    private void SetupVictimEffects(EntityUid victim)
    {
        // Force prone
        if (HasComp<StandingStateComponent>(victim))
        {
            _standing.Down(victim);
        }

        if (!HasComp<BlockMovementComponent>(victim))
        {
            AddComp<BlockMovementComponent>(victim);
        }

        EnsureComp<MumbleAccentComponent>(victim);
        EnsureComp<TemporaryBlindnessComponent>(victim);
    }

    private void OnCocoonContainerShutdown(EntityUid uid, CocoonContainerComponent component, ComponentShutdown args)
    {
        if (component.Victim == null || !Exists(component.Victim.Value))
            return;

        var victim = component.Victim.Value;

        // Remove effects from victim
        if (HasComp<BlockMovementComponent>(victim))
            RemComp<BlockMovementComponent>(victim);

        if (HasComp<MumbleAccentComponent>(victim))
        {
            RemComp<MumbleAccentComponent>(victim);
        }

        if (HasComp<TemporaryBlindnessComponent>(victim))
        {
            RemComp<TemporaryBlindnessComponent>(victim);
        }
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

        // Check if target is already in a cocoon container
        if (_container.TryGetContainingContainer(target, out var existingContainer) &&
            HasComp<CocoonContainerComponent>(existingContainer.Owner))
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

        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        // Check if target is already in a cocoon container
        if (_container.TryGetContainingContainer(target, out var container) &&
            HasComp<CocoonContainerComponent>(container.Owner))
        {
            _popups.PopupEntity(Loc.GetString("arachnid-wrap-already"), performer, performer);
            return;
        }

        if (!_blocker.CanInteract(performer, target))
            return;

        // Only consume hunger if the entity has a HungerComponent
        if (TryComp<Content.Shared.Nutrition.Components.HungerComponent>(performer, out var hunger))
        {
            _hunger.ModifyHunger(performer, -component.HungerCost);
        }

        // Spawn cocoon container at target's position
        var targetCoords = _transform.GetMapCoordinates(target);
        var cocoonContainer = Spawn("CocoonContainer", targetCoords);
        
        // Set up the container component
        if (!TryComp<CocoonContainerComponent>(cocoonContainer, out var cocoonComp))
        {
            Log.Error("CocoonContainer spawned without CocoonContainerComponent!");
            Del(cocoonContainer);
            return;
        }

        cocoonComp.Victim = target;
        Dirty(cocoonContainer, cocoonComp);

        // Drop all items from victim's hands before inserting
        if (TryComp<HandsComponent>(target, out var hands))
        {
            foreach (var hand in _hands.EnumerateHands(target, hands))
            {
                if (hand.HeldEntity != null)
                {
                    _hands.TryDrop(target, hand, checkActionBlocker: false);
                }
            }
        }

        // Insert victim into container
        if (!_entityStorage.Insert(target, cocoonContainer))
        {
            Log.Error($"Failed to insert {target} into cocoon container {cocoonContainer}");
            Del(cocoonContainer);
            return;
        }

        // Apply effects to victim after insertion (ComponentStartup may have fired before victim was set)
        SetupVictimEffects(target);

        // Play ziptie sound for everyone within 10 meters
        var filter = Filter.Empty().AddInRange(targetCoords, 10f);
        var entityCoords = _transform.ToCoordinates(targetCoords);
        _audio.PlayStatic(new SoundPathSpecifier("/Audio/Items/Handcuffs/rope_end.ogg"), filter, entityCoords, true);

        _popups.PopupEntity(Loc.GetString("arachnid-wrap-complete-user", ("target", target)), performer, performer);
        _popups.PopupEntity(Loc.GetString("arachnid-wrap-complete-target"), target, target, PopupType.LargeCaution);

        args.Handled = true;
    }

    private void OnUnwrapDoAfter(EntityUid uid, CocoonContainerComponent component, ref UnwrapDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        // Play cocoon removal sound for everyone within 10 meters
        PlayCocoonRemovalSound(uid);

        // Remove victim from container
        if (component.Victim != null && Exists(component.Victim.Value))
        {
            _entityStorage.Remove(component.Victim.Value, uid);
        }

        // Delete the container
        Del(uid);

        if (component.Victim != null && Exists(component.Victim.Value))
        {
            _popups.PopupEntity(Loc.GetString("arachnid-unwrap-user", ("target", component.Victim.Value)), args.User, args.User);
            _popups.PopupEntity(Loc.GetString("arachnid-unwrap-target", ("user", args.User)), component.Victim.Value, component.Victim.Value);
        }
    }

    private void OnGetUnwrapVerb(EntityUid uid, CocoonContainerComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        // Must be in range, must be able to interact
        if (!args.CanAccess)
            return;

        if (!args.CanInteract)
            return;

        // Unwrapping verb
        var unwrapVerb = new InteractionVerb
        {
            Text = Loc.GetString("arachnid-unwrap-verb", ("target", component.Victim ?? uid)),
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

                var targetName = component.Victim != null && Exists(component.Victim.Value) ? component.Victim.Value : uid;
                _popups.PopupEntity(Loc.GetString("arachnid-unwrap-start-user", ("target", targetName)), args.User, args.User);
                if (component.Victim != null && Exists(component.Victim.Value))
                {
                    _popups.PopupEntity(Loc.GetString("arachnid-unwrap-start-target", ("user", args.User)), component.Victim.Value, component.Victim.Value);
                }
            }
        };
        args.Verbs.Add(unwrapVerb);
    }

    /// <summary>
    /// Breaks the cocoon container and releases the victim.
    /// </summary>
    private void BreakCocoon(Entity<CocoonContainerComponent> cocoon)
    {
        PlayCocoonRemovalSound(cocoon);

        // Remove victim from container before deleting
        if (cocoon.Comp.Victim != null && Exists(cocoon.Comp.Victim.Value))
        {
            _entityStorage.Remove(cocoon.Comp.Victim.Value, cocoon);
            _popups.PopupEntity(Loc.GetString("arachnid-cocoon-broken"), cocoon.Comp.Victim.Value, cocoon.Comp.Victim.Value, PopupType.LargeCaution);
        }

        // Delete the container
        Del(cocoon);
    }

}
