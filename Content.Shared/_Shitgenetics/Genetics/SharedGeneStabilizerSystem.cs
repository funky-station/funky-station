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
using Content.Shared.Light.Components;
using Content.Shared.Clumsy;
using Content.Shared.Temperature.Components;

namespace Content.Shared.Genetics
{
    public abstract partial class SharedGeneStabilizerSystem : EntitySystem // I HAVE NO FUCKING IDEA WHAT IM FUCKING DOING, HELP ME
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
        [Dependency] private readonly SharedPointLightSystem _pointlight = default!;

        public override void Initialize()
        {
            base.Initialize();

            SubscribeLocalEvent<GeneStabilizerComponent, MeleeHitEvent>(OnMeleeInject);
            SubscribeLocalEvent<GeneStabilizerComponent, AddInjectDoAfterEvent>(OnAddDNADoAfter);
        }
        private void OnMeleeInject(EntityUid uid, GeneStabilizerComponent component, MeleeHitEvent args)
        {
            if (!args.HitEntities.Any())
                return;

            TryInjecting(args.User, args.HitEntities.First(), uid, component);
            args.Handled = true;
        }
        public bool TryInjecting(EntityUid user, EntityUid target, EntityUid item, GeneStabilizerComponent? injectorComponent = null, CuffableComponent? cuffable = null)
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

        private void OnAddDNADoAfter(EntityUid uid, GeneStabilizerComponent component, AddInjectDoAfterEvent args)
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

                EntityManager.DeleteEntity(uid);

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

        public bool TryAddMutation(EntityUid target, EntityUid user, EntityUid item, CuffableComponent? component = null, GeneStabilizerComponent? gene = null, MutationComponent? mutation = null)
        {

            if (!_interaction.InRangeUnobstructed(item, target))
                return false;
            //honestly, i just turn on a var so the comp basically deletes itself for you on the next tick. this makes it more easy than deleting server comps from a shared script.

            if (TryComp<MutationComponent>(target, out var mutations))
            {
                if (mutations != null) mutations.Cancel = true;
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

