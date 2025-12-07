// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Examine;
using Content.Shared.Hands;
using Content.Shared.Heretic;
using Content.Shared.Inventory;

namespace Content.Server.Heretic.EntitySystems;

public sealed partial class HereticMagicItemSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HereticMagicItemComponent, CheckMagicItemEvent>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, HeldRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);
        SubscribeLocalEvent<HereticMagicItemComponent, InventoryRelayedEvent<CheckMagicItemEvent>>(OnCheckMagicItem);

    }

    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref CheckMagicItemEvent args)
        => args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref HeldRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;
    private void OnCheckMagicItem(Entity<HereticMagicItemComponent> ent, ref InventoryRelayedEvent<CheckMagicItemEvent> args)
        => args.Args.Handled = true;
}
