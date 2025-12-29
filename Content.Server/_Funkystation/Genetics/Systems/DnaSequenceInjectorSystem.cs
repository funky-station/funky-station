using Content.Server.DoAfter;
using Content.Server.Popups;
using Content.Server._Funkystation.Genetics.Components;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Server._Funkystation.Genetics;
using System.Linq;
using Content.Shared.Interaction;
using Content.Shared.Examine;
using Content.Server._Funkystation.Genetics.Systems;
using Content.Shared._Funkystation.Genetics.Prototypes;
using Robust.Shared.Prototypes;
using Content.Shared._Funkystation.Genetics;
using Content.Shared.Popups;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed class DNASequenceInjectorSystem : EntitySystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;
    [Dependency] private readonly GeneticShuffleSystem _shuffle = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DnaSequenceInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<DnaSequenceInjectorComponent, UseInHandEvent>(OnUseInHand);
        SubscribeLocalEvent<DnaSequenceInjectorComponent, DNASequenceInjectorDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<DnaSequenceInjectorComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(EntityUid uid, DnaSequenceInjectorComponent comp, ExaminedEvent args)
    {
        if (comp.MutationId == null)
        {
            args.PushMarkup(Loc.GetString("dna-injector-examine-empty"));
            return;
        }

        var type = comp.IsMutator ? "mutator" : "activator";
        var name = _prototype.TryIndex<GeneticMutationPrototype>(comp.MutationId, out var proto)
            ? proto.Name
            : comp.MutationId;

        args.PushMarkup(Loc.GetString($"dna-injector-examine-{type}", ("mutation", name)));
    }

    private void OnAfterInteract(EntityUid uid, DnaSequenceInjectorComponent comp, AfterInteractEvent args)
    {
        if (args.Target is not { Valid: true } target || !args.CanReach || args.Handled)
            return;

        if (comp.MutationId == null)
        {
            args.Handled = true;
            return;
        }

        var user = args.User;

        var doAfterArgs = new DoAfterArgs(EntityManager, user, 2f, new DNASequenceInjectorDoAfterEvent(), uid, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTarget
        };

        if (user != target)
        {
            _popup.PopupEntity(Loc.GetString("dna-injector-start-other", ("user", Name(user))), target, target);
            _popup.PopupEntity(Loc.GetString("dna-injector-start", ("user", Name(user))), user, user);
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("dna-injector-start-self"), user, user);
        }

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnUseInHand(EntityUid uid, DnaSequenceInjectorComponent comp, UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (comp.MutationId == null)
        {
            args.Handled = true;
            return;
        }

        var user = args.User;
        var target = args.User;

        var doAfterArgs = new DoAfterArgs(EntityManager, user, 2f, new DNASequenceInjectorDoAfterEvent(), uid, target)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            BreakOnHandChange = true,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameTarget
        };

        _popup.PopupEntity(Loc.GetString("dna-injector-start-self"), user, user);

        _doAfter.TryStartDoAfter(doAfterArgs);
        args.Handled = true;
    }

    private void OnDoAfter(EntityUid uid, DnaSequenceInjectorComponent comp, DNASequenceInjectorDoAfterEvent args)
    {
        if (args.Cancelled || args.Target is not { } target || args.Handled)
            return;

        if (TryInject(uid, target, args.User, comp))
            args.Handled = true;
    }

    private bool TryInject(EntityUid injector, EntityUid targetUid, EntityUid user, DnaSequenceInjectorComponent comp)
    {
        if (comp.MutationId is not { } mutationId)
        {
            return false;
        }

        if (!TryComp<GeneticsComponent>(targetUid, out var genetics))
        {
            return false;
        }

        if (!_prototype.TryIndex<GeneticMutationPrototype>(mutationId, out var proto))
        {
            return false;
        }

        if (!_shuffle.TryGetSlot(mutationId, out var slot))
        {
            // This should never happen so just delete it and give them nothing
            _popup.PopupEntity(Loc.GetString("dna-injector-no-effect"), targetUid, user);
            Del(injector);
            return false;
        }

        bool success;

        if (comp.IsMutator)
        {
            success = _genetics.TryAddMutation(targetUid, genetics, mutationId) &&
                      _genetics.TryActivateMutation(targetUid, genetics, mutationId);
        }
        else
        {
            success = _genetics.TryActivateMutation(targetUid, genetics, mutationId);
        }

        if (!success)
        {
            _popup.PopupEntity(Loc.GetString("dna-injector-no-effect"), targetUid, user);
        }

        var empty = Spawn("DNAInjectorEmpty", Transform(injector).Coordinates);

        if (TryComp<HandsComponent>(user, out var hands) && hands.ActiveHandEntity == injector)
        {
            _hands.DoDrop(user, hands.ActiveHand!, false, hands);
            _hands.DoPickup(user, hands.ActiveHand!, empty);
        }

        Del(injector);
        return true;
    }
}
