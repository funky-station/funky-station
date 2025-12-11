using Content.Server.DeviceLinking.Systems;
using Content.Server.DoAfter;
using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared._Funkystation.Genetics.Systems;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Verbs;

namespace Content.Server._Funkystation.Genetics.Systems;

public sealed class DnaScannerSystem : SharedDnaScannerSystem
{
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaScannerComponent, DragDropTargetEvent>(OnDragDrop);
        SubscribeLocalEvent<DnaScannerComponent, DnaScannerDragFinished>(OnDragFinished);
        SubscribeLocalEvent<DnaScannerComponent, GetVerbsEvent<AlternativeVerb>>(AddEjectVerb);
    }

    private void OnDragDrop(EntityUid uid, DnaScannerComponent component, ref DragDropTargetEvent args)
    {
        if (component.BodyContainer.ContainedEntity != null || args.Handled)
            return;

        args.Handled = true;

        var doAfter = new DoAfterArgs(EntityManager, args.User, component.EntryDelay, new DnaScannerDragFinished(), uid, target: args.Dragged, used: uid)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            MovementThreshold = 0.5f,
            NeedHand = false
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDragFinished(EntityUid uid, DnaScannerComponent component, DnaScannerDragFinished args)
    {
        if (args.Cancelled || args.Target is not { Valid: true } target)
            return;

        InsertBody(uid, target, component);
    }

    private void AddEjectVerb(EntityUid uid, DnaScannerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || component.BodyContainer.ContainedEntity == null)
            return;

        args.Verbs.Add(new AlternativeVerb
        {
            Text = "Eject occupant",
            Category = VerbCategory.Eject,
            Priority = 10,
            Act = () => EjectBody(uid, component)
        });
    }
}
