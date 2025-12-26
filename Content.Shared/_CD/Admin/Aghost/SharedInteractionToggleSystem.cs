// SPDX-FileCopyrightText: 2025 dffdff2423 <dffdff2423@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Content.Shared.Interaction.Events;

namespace Content.Shared._CD.Admin.Aghost;

public abstract class SharedInteractionToggleSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alertsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<InteractionToggleableComponent, InteractionAttemptEvent>(OnAttemptInteract);
        SubscribeLocalEvent<InteractionToggleableComponent, ToggleInteractionEvent>(OnToggleInteraction);
        SubscribeLocalEvent<InteractionToggleableComponent, ComponentInit>(OnCompInit);
    }

    private void OnAttemptInteract(Entity<InteractionToggleableComponent> ent, ref InteractionAttemptEvent args)
    {
        if (ent.Comp.BlockInteraction)
            args.Cancelled = true;

    }

    private void OnToggleInteraction(Entity<InteractionToggleableComponent> ent, ref ToggleInteractionEvent args)
    {
        ent.Comp.BlockInteraction = !ent.Comp.BlockInteraction;

        if (TryComp<AlertsComponent>(ent.Owner, out var alerts))
        {
            _alertsSystem.ShowAlert(
                (ent.Owner, alerts),
                ent.Comp.ToggleAlertProtoId,
                (short) (ent.Comp.BlockInteraction ? 1 : 0)
            );
        }

        Dirty(ent);
    }
    private void OnCompInit(Entity<InteractionToggleableComponent> ent, ref ComponentInit args)
    {
        if (TryComp<AlertsComponent>(ent.Owner, out var alerts))
        {
            _alertsSystem.ShowAlert(
                (ent.Owner, alerts),
                ent.Comp.ToggleAlertProtoId,
                0
            );
        }
    }
}
