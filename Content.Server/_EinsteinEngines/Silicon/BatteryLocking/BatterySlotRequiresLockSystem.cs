// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Containers.ItemSlots;
using Content.Shared.Lock;
using Content.Shared.Popups;
using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.IdentityManagement;

namespace Content.Server._EinsteinEngines.Silicons.BatteryLocking;

public sealed class BatterySlotRequiresLockSystem : EntitySystem

{
    [Dependency] private readonly ItemSlotsSystem _itemSlotsSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BatterySlotRequiresLockComponent, LockToggledEvent>(LockToggled);
        SubscribeLocalEvent<BatterySlotRequiresLockComponent, LockToggleAttemptEvent>(LockToggleAttempted);

    }
    private void LockToggled(EntityUid uid, BatterySlotRequiresLockComponent component, LockToggledEvent args)
    {
        if (!TryComp<LockComponent>(uid, out var lockComp)
            || !TryComp<ItemSlotsComponent>(uid, out var itemslots)
            || !_itemSlotsSystem.TryGetSlot(uid, component.ItemSlot, out var slot, itemslots))
            return;

        _itemSlotsSystem.SetLock(uid, slot, lockComp.Locked, itemslots);
    }

    private void LockToggleAttempted(EntityUid uid, BatterySlotRequiresLockComponent component, LockToggleAttemptEvent args)
    {
        if (args.User == uid || !HasComp<SiliconComponent>(uid))
            return;

        _popupSystem.PopupEntity(Loc.GetString("batteryslotrequireslock-component-alert-owner", ("user", Identity.Entity(args.User, EntityManager))), uid, uid, PopupType.Large);
    }

}
