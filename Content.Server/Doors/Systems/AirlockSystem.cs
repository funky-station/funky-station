using Content.Server.Power.Components;
using Content.Server.Wires;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Emp;
using Content.Shared.Interaction;
using Content.Shared.Power;
using Content.Shared.Wires;
using Robust.Shared.Player;

namespace Content.Server.Doors.Systems;

public sealed class AirlockSystem : SharedAirlockSystem
{
    [Dependency] private readonly WiresSystem _wiresSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AirlockComponent, SignalReceivedEvent>(OnSignalReceived);
        SubscribeLocalEvent<AirlockComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<AirlockComponent, ActivateInWorldEvent>(OnActivate, before: new[] { typeof(DoorSystem) });

        // EMP event subscriptions
        SubscribeLocalEvent<AirlockComponent, EmpPulseEvent>(OnEmpPulse);
        SubscribeLocalEvent<AirlockComponent, EmpDisabledRemovedEvent>(OnEmpFinished);
    }

    private void OnSignalReceived(EntityUid uid, AirlockComponent component, ref SignalReceivedEvent args)
    {
        if (args.Port == component.AutoClosePort && component.AutoClose)
        {
            component.AutoClose = false;
            Dirty(uid, component);
        }
    }

    private void OnPowerChanged(EntityUid uid, AirlockComponent component, ref PowerChangedEvent args)
    {
        component.Powered = args.Powered;
        Dirty(uid, component);

        if (!TryComp(uid, out DoorComponent? door))
            return;

        if (!args.Powered)
        {
            // stop any scheduled auto-closing
            if (door.State == DoorState.Open)
                DoorSystem.SetNextStateChange(uid, null);
        }
        else
        {
            UpdateAutoClose(uid, door: door);
        }
    }

    private void OnActivate(EntityUid uid, AirlockComponent component, ActivateInWorldEvent args)
    {
        if (args.Handled || !args.Complex)
            return;

        if (TryComp<WiresPanelComponent>(uid, out var panel) &&
            panel.Open &&
            TryComp<ActorComponent>(args.User, out var actor))
        {
            if (TryComp<WiresPanelSecurityComponent>(uid, out var wiresPanelSecurity) &&
                !wiresPanelSecurity.WiresAccessible)
                return;

            _wiresSystem.OpenUserInterface(uid, actor.PlayerSession);
            args.Handled = true;
            return;
        }

        if (component.KeepOpenIfClicked && component.AutoClose)
        {
            // Disable auto close
            component.AutoClose = false;
            Dirty(uid, component);
        }
    }

    // Airlocks get disabled by EMP pulses
    private void OnEmpPulse(EntityUid uid, AirlockComponent comp, ref EmpPulseEvent args)
    {
        args.Affected = true;
        args.Disabled = true;

        comp.Powered = false;
        comp.ForceDisabled = true;  // Prevent power updates from setting powered back to true
        Dirty(uid, comp);
    }

    private void OnEmpFinished(EntityUid uid, AirlockComponent comp, ref EmpDisabledRemovedEvent args)
    {
        comp.ForceDisabled = false;
        Dirty(uid, comp);

        if (TryComp<ApcPowerReceiverComponent>(uid, out var powerComp))
            powerComp.Recalculate = true;   // Re-check power state in case it stopped getting powered/wasn't powered in the first place
    }
}
