// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Client.Items;
using Content.Shared._DV.Surgery;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Client._DV.Surgery;

/// <summary>
/// This shows the item status for dirty surgery tools.
/// </summary>
public sealed class SurgeryCleanStatusSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;

    public override void Initialize()
    {
        base.Initialize();

        Subs.ItemStatus<SurgeryDirtinessComponent>(ent => new SurgeryDirtinessItemStatus(ent, EntityManager, _inventory, _container));
    }
}
