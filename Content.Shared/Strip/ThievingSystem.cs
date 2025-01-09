using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Mindshield.Components;
using Content.Shared.Strip.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Strip;

public sealed class ThievingSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alertsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThievingComponent, BeforeStripEvent>(OnBeforeStrip);
        SubscribeLocalEvent<ThievingComponent, ComponentInit>(OnCompInit);
        SubscribeLocalEvent<ThievingComponent, ComponentRemove>(OnCompRemove);
        SubscribeLocalEvent<ThievingComponent, InventoryRelayedEvent<BeforeStripEvent>>((e, c, ev) => OnBeforeStrip(e, c, ev.Args));
        SubscribeLocalEvent<ThievingComponent, ThievingToggleEvent>(OnThievingToggle);
    }

    private void OnCompInit(EntityUid uid, ThievingComponent comp, ComponentInit args)
    {
        comp.DefaultTimeReduction = comp.StripTimeReduction;

        _alertsSystem.ShowAlert(uid, "Thieving");
    }

    private void OnCompRemove(EntityUid uid, ThievingComponent comp, ComponentRemove args)
    {
        _alertsSystem.ClearAlert(uid, "Thieving");
    }

    private void OnThievingToggle(Entity<ThievingComponent> ent, ref ThievingToggleEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Stealthy = !ent.Comp.Stealthy;
        ent.Comp.StripTimeReduction = ent.Comp.Stealthy ? ent.Comp.DefaultTimeReduction : TimeSpan.Zero;

        args.Handled = true;
    }

    private void OnBeforeStrip(EntityUid uid, ThievingComponent component, BeforeStripEvent args)
    {
        args.Stealth |= component.Stealthy;
        args.Additive -= component.StripTimeReduction;
    }
}
