// SPDX-FileCopyrightText: 2025 Whatstone <166147148+whatston3@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._NF.Manufacturing.Components;
using Content.Shared.Containers.ItemSlots;

namespace Content.Shared._NF.Manufacturing.EntitySystems;

/// <summary>
/// Consumes large quantities of power, scales excessive overage down to reasonable values.
/// Spawns entities when thresholds reached.
/// </summary>
public abstract partial class SharedEntitySpawnPowerConsumerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntitySpawnPowerConsumerComponent, ItemSlotInsertAttemptEvent>(OnItemSlotInsertAttempt);
    }

    private void OnItemSlotInsertAttempt(Entity<EntitySpawnPowerConsumerComponent> ent, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.User != null)
            args.Cancelled = true;
    }
}
