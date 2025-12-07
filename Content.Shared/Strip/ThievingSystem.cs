// SPDX-FileCopyrightText: 2022 Emisse <99158783+Emisse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Zergologist <114537969+Chedd-Error@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Content.Shared.Inventory;
using Content.Shared.Strip.Components;

namespace Content.Shared.Strip;

public sealed class ThievingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

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

        _alertsSystem.ShowAlert(uid, comp.ThievingAlertProtoId, 2);
    }

    private void OnCompRemove(EntityUid uid, ThievingComponent comp, ComponentRemove args)
    {
        _alertsSystem.ClearAlert(uid, comp.ThievingAlertProtoId);
    }

    private void OnThievingToggle(Entity<ThievingComponent> ent, ref ThievingToggleEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Stealthy = !ent.Comp.Stealthy;
        ent.Comp.StripTimeReduction = ent.Comp.Stealthy ? ent.Comp.DefaultTimeReduction : TimeSpan.Zero;

        switch (ent.Comp.Stealthy)
        {
            case false:
                _alertsSystem.ShowAlert(ent.Owner, ent.Comp.ThievingAlertProtoId, 1);
                break;

            case true:
                _alertsSystem.ShowAlert(ent.Owner, ent.Comp.ThievingAlertProtoId, 2);
                break;
        }

        args.Handled = true;
    }

    private void OnBeforeStrip(EntityUid uid, ThievingComponent component, BeforeStripEvent args)
    {
        args.Stealth |= component.Stealthy;
        args.Additive -= component.StripTimeReduction;
    }
}
