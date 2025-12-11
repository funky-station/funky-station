using Content.Shared._Funkystation.Genetics.Components;
using Content.Shared.Body.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.DoAfter;
using Content.Shared.DragDrop;
using Content.Shared.Power;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Standing;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;
using Robust.Shared.Toolshed.Commands.Values;

namespace Content.Shared._Funkystation.Genetics.Systems;

public abstract class SharedDnaScannerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedDnaScannerConsoleSystem _consoleSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaScannerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DnaScannerComponent, CanDropTargetEvent>(OnCanDrop);
        SubscribeLocalEvent<DnaScannerComponent, EntInsertedIntoContainerMessage>(OnInsert);
        SubscribeLocalEvent<DnaScannerComponent, EntRemovedFromContainerMessage>(OnRemove);
        SubscribeLocalEvent<DnaScannerComponent, PowerChangedEvent>(OnPowerChanged);

        SubscribeLocalEvent<InsideDnaScannerComponent, EntGotRemovedFromContainerMessage>(OnRemovedFromContainer);
    }

    private void OnInit(EntityUid uid, DnaScannerComponent component, ComponentInit args)
    {
        component.BodyContainer = (ContainerSlot) _container.GetContainer(uid, "scanner-body");
        UpdateAppearance(uid, component);
    }

    private void OnCanDrop(EntityUid uid, DnaScannerComponent component, ref CanDropTargetEvent args)
    {
        args.CanDrop = HasComp<BodyComponent>(args.Dragged);
        args.Handled = true;
    }
    private void OnPowerChanged(EntityUid uid, DnaScannerComponent component, PowerChangedEvent args)
    {
        UpdateAppearance(uid, component);
    }

    private void OnInsert(EntityUid uid, DnaScannerComponent component, EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID == "scanner-body" && args.Container == component.BodyContainer)
        {
            EnsureComp<InsideDnaScannerComponent>(args.Entity);
            _standing.Stand(args.Entity, force: true);
            UpdateAppearance(uid, component);
            NotifyLinkedConsoles(uid, args.Entity);
        }
    }

    private void OnRemove(EntityUid uid, DnaScannerComponent component, EntRemovedFromContainerMessage args)
    {
        if (args.Container != component.BodyContainer)
            return;

        RemComp<InsideDnaScannerComponent>(args.Entity);
        UpdateAppearance(uid, component);
        NotifyLinkedConsoles(uid, null);
    }

    private void OnRemovedFromContainer(EntityUid uid, InsideDnaScannerComponent component, EntGotRemovedFromContainerMessage args)
    {
        RemCompDeferred<InsideDnaScannerComponent>(uid);
    }

    protected void UpdateAppearance(EntityUid uid, DnaScannerComponent? component = null, AppearanceComponent? appearance = null)
    {
        if (!Resolve(uid, ref component))
            return;

        var occupied = component.BodyContainer.ContainedEntity != null;

        var powered = _powerReceiver.IsPowered(uid);

        var state = (occupied, powered) switch
        {
            (true, true) => DnaScannerState.Occupied,
            (false, true) => DnaScannerState.Empty,
            (true, false) => DnaScannerState.OccupiedUnpowered,
            (false, false) => DnaScannerState.EmptyUnpowered,
        };

        _appearance.SetData(uid, DnaScannerVisuals.State, state);
    }

    public bool InsertBody(EntityUid uid, EntityUid target, DnaScannerComponent component)
    {
        if (component.BodyContainer.ContainedEntity != null)
            return false;

        var xform = Transform(target);
        _container.Insert((target, xform), component.BodyContainer);
        return true;
    }

    public virtual EntityUid? EjectBody(EntityUid uid, DnaScannerComponent? component)
    {
        if (!Resolve(uid, ref component) || component.BodyContainer.ContainedEntity is not { Valid: true } body)
            return null;

        _container.Remove(body, component.BodyContainer);
        UpdateAppearance(uid, component);
        return body;
    }

    private void NotifyLinkedConsoles(EntityUid scannerUid, EntityUid? occupant)
    {
        if (_consoleSystem == null)
            return;

        if (!TryComp<DeviceLinkSinkComponent>(scannerUid, out var sink))
            return;

        foreach (var consoleUid in sink.LinkedSources)
        {
            if (occupant is { Valid: true } occ)
                _consoleSystem.SetSubject(consoleUid, occ);
            else
                _consoleSystem.ClearSubject(consoleUid);
        }
    }
}

[Serializable, NetSerializable]
public sealed partial class DnaScannerDragFinished : SimpleDoAfterEvent { }
