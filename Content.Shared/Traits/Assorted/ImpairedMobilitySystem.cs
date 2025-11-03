// SPDX-FileCopyrightText: 2025 Mora <46364955+trixxedheart@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Wieldable.Components;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Handles <see cref="ImpairedMobilityComponent"/>
/// </summary>
public sealed class ImpairedMobilitySystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speedModifier = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ImpairedMobilityComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<ImpairedMobilityComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ImpairedMobilityComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
    }

    private void OnInit(Entity<ImpairedMobilityComponent> ent, ref ComponentInit args)
    {
        _speedModifier.RefreshMovementSpeedModifiers(ent);
    }

    private void OnShutdown(Entity<ImpairedMobilityComponent> ent, ref ComponentShutdown args)
    {
        _speedModifier.RefreshMovementSpeedModifiers(ent);
    }

    // Handles movement speed for entities with impaired mobility.
    // Applies a speed penalty, but counteracts it if the entity is holding a non-wielded mobility aid.
    private void OnRefreshMovementSpeed(Entity<ImpairedMobilityComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (HasMobilityAid(ent.Owner))
            return;

        args.ModifySpeed(ent.Comp.SpeedModifier);
    }

    // Checks if the entity is holding any non-wielded mobility aids.
    private bool HasMobilityAid(Entity<HandsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return false;

        foreach (var held in _hands.EnumerateHeld(entity))
        {
            if (!HasComp<MobilityAidComponent>(held))
                continue;

            // Makes sure it's not wielded yet
            if (TryComp<WieldableComponent>(held, out var wieldable) && wieldable.Wielded)
                continue;

            return true;
        }

        return false;
    }
}
