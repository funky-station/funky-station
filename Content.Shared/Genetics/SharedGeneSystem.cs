using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Cuffs.Components;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Timing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Serialization;
using Content.Shared.Genetics.Components;
using Content.Shared.Mutations;

namespace Content.Shared.Genetics
{
    public abstract partial class SharedGeneSystem : EntitySystem // I HAVE NO FUCKING IDEA WHAT IM FUCKING DOING, HELP ME
    {

        [Dependency] private readonly IComponentFactory _componentFactory = default!;
        [Dependency] private readonly INetManager _net = default!;
        [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;
        [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly SharedAudioSystem _audio = default!;
        [Dependency] private readonly SharedContainerSystem _container = default!;
        [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
        [Dependency] private readonly SharedHandsSystem _hands = default!;
        [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
        [Dependency] private readonly SharedInteractionSystem _interaction = default!;
        [Dependency] private readonly SharedPopupSystem _popup = default!;
        [Dependency] private readonly SharedTransformSystem _transform = default!;
        [Dependency] private readonly UseDelaySystem _delay = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GeneinjectorComponent, MeleeHitEvent>(OnMeleeInject);
            SubscribeLocalEvent<GeneinjectorComponent, AddInjectDoAfterEvent>(OnAddDNADoAfter);
        }
        private void OnMeleeInject(EntityUid uid, GeneinjectorComponent component, MeleeHitEvent args)
        {
            if (!args.HitEntities.Any())
                return;

            TryInjecting(args.User, args.HitEntities.First(), uid, component);
            args.Handled = true;
        }
        public bool TryInjecting(EntityUid user, EntityUid target, EntityUid item, GeneinjectorComponent? injectorComponent = null, CuffableComponent? cuffable = null)
        {
            if (!Resolve(item, ref injectorComponent) || !Resolve(target, ref cuffable, false)) //use the fartass cuffable cuz it works
                return false;

            var injectTime = injectorComponent.InjectTime;

            if (HasComp<StunnedComponent>(target))
                injectTime = MathF.Max(0.1f, injectTime - injectorComponent.StunBonus);

            var doAfterEventArgs = new DoAfterArgs(EntityManager, user, injectTime, new AddInjectDoAfterEvent(), item, target, item)
            {
                BreakOnMove = true,
                BreakOnWeightlessMove = false,
                BreakOnDamage = true,
                NeedHand = true,
                DistanceThreshold = 1f
            };

            if (!_doAfter.TryStartDoAfter(doAfterEventArgs))
                return true;

            _popup.PopupEntity(Loc.GetString("geneinjector-component-start-injecting-observer",
                    ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                target, Filter.Pvs(target, entityManager: EntityManager)
                    .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);

            if (target == user)
            {
                _popup.PopupClient(Loc.GetString("geneinjector-component-target-self"), user, user);
            }
            else
            {
                _popup.PopupClient(Loc.GetString("geneinjector-component-start-injecting-target-message",
                    ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                _popup.PopupEntity(Loc.GetString("geneinjector-component-start-injecting-by-other-message",
                    ("otherName", Identity.Name(user, EntityManager, target))), target, target);
            }
            return true;
        }

        private void OnAddDNADoAfter(EntityUid uid, GeneinjectorComponent component, AddInjectDoAfterEvent args)
        {
            var user = args.Args.User;

            if (!TryComp<CuffableComponent>(args.Args.Target, out var cuffable))
                return;

            var target = args.Args.Target.Value;

            if (args.Handled)
                return;
            args.Handled = true;

            if (!args.Cancelled && TryAddMutation(target, user, uid, cuffable))
            {

                _popup.PopupEntity(Loc.GetString("geneinjector-component-inject-observer-success-message",
                        ("user", Identity.Name(user, EntityManager)), ("target", Identity.Name(target, EntityManager))),
                    target, Filter.Pvs(target, entityManager: EntityManager)
                        .RemoveWhere(e => e.AttachedEntity == target || e.AttachedEntity == user), true);


                if (target == user)
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-self-success-message"), user, user);
                    _adminLog.Add(LogType.Action, LogImpact.Medium,
                        $"{ToPrettyString(user):player} has injected himself");

                    EntityManager.DeleteEntity(uid);
                }
                else
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-other-success-message",
                        ("otherName", Identity.Name(target, EntityManager, user))), user, user);
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-by-other-success-message",
                        ("otherName", Identity.Name(user, EntityManager, target))), target, target);
                    _adminLog.Add(LogType.Action, LogImpact.Medium,
                        $"{ToPrettyString(user):player} has injected {ToPrettyString(target):player}");

                    EntityManager.DeleteEntity(uid);
                }
            }
            else
            {
                if (target == user)
                {
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-self-message"), user, user);
                }
                else
                {

                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-message",
                        ("targetName", Identity.Name(target, EntityManager, user))), user, user);
                    _popup.PopupClient(Loc.GetString("geneinjector-component-inject-interrupt-other-message",
                        ("otherName", Identity.Name(user, EntityManager, target))), target, target);
                }
            }
        }

        public bool TryAddMutation(EntityUid target, EntityUid user, EntityUid item, CuffableComponent? component = null, GeneinjectorComponent? gene = null, InjectionPresetComponent? inject = null, MutationComponent? mutation = null)
        {

            if (!_interaction.InRangeUnobstructed(item, target))
                return false;

            EnsureComp<MutationComponent>(target);

            if (TryComp<MutationComponent>(target, out var mutations))
            {
                if (TryComp<InjectionPresetComponent>(item, out var injectpreset))
                {
                    if ((mutations != null) && (injectpreset != null)) //to anyone whos like trying to make mutations, im so sorry.
                    {
                        if (injectpreset.AcidVomit) mutations.AcidVomit = true; //its 3 am, im fucking tired, im just gonna hardcode it, im so sorry taydeo, I have dishonored the john space bloodline.
                        if (injectpreset.BloodVomit) mutations.BloodVomit = true;
                        if (injectpreset.BlueLight) mutations.BlueLight = true;
                        if (injectpreset.BreathingImmune) mutations.BreathingImmune = true;
                        if (injectpreset.BZFarter) mutations.BZFarter = true;
                        if (injectpreset.Clumsy) mutations.Clumsy = true;
                        if (injectpreset.FireSkin) mutations.FireSkin = true;
                        if (injectpreset.Light) mutations.Light = true;
                        if (injectpreset.OkayAccent) mutations.OkayAccent = true;
                        if (injectpreset.PlasmaFarter) mutations.PlasmaFarter = true;
                        if (injectpreset.PressureImmune) mutations.PressureImmune = true;
                        if (injectpreset.Prickmode) mutations.Prickmode = true;
                        if (injectpreset.RadiationImmune) mutations.RadiationImmune = true;
                        if (injectpreset.RedLight) mutations.RedLight = true;
                        if (injectpreset.RGBLight) mutations.RGBLight = true;
                        if (injectpreset.TempImmune) mutations.TempImmune = true;
                        if (injectpreset.TritFarter) mutations.TritFarter = true;
                        if (injectpreset.Twitch) mutations.Twitch = true;
                        if (injectpreset.Vomit) mutations.Vomit = true;
                    }
                }
            }
            return true;
        }

        public IReadOnlyList<EntityUid> GetAllCuffs(CuffableComponent component)
        {
            return component.Container.ContainedEntities;
        }

        [Serializable, NetSerializable]
        private sealed partial class AddInjectDoAfterEvent : SimpleDoAfterEvent
        {
        }
    }
}

