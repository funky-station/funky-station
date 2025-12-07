// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clothing.Components;
using Content.Shared.Inventory;
using Content.Shared.Movement.Events;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class SpeedModifierContactCapClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpeedModifierContactCapClothingComponent, InventoryRelayedEvent<GetSpeedModifierContactCapEvent>>(OnGetMaxSlow);
    }

    private void OnGetMaxSlow(Entity<SpeedModifierContactCapClothingComponent> ent, ref InventoryRelayedEvent<GetSpeedModifierContactCapEvent> args)
    {
        args.Args.SetIfMax(ent.Comp.MaxContactSprintSlowdown, ent.Comp.MaxContactWalkSlowdown);
    }
}
