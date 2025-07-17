// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Flashbang;

public sealed class GetFlashbangedEvent(float range) : EntityEventArgs, IInventoryRelayEvent
{
    public float ProtectionRange = range;

    public SlotFlags TargetSlots => SlotFlags.EARS | SlotFlags.HEAD;
}
public sealed class AreaFlashEvent(float range, float distance, EntityUid target) : EntityEventArgs
{
    public float Range = range;

    public float Distance = distance;

    public EntityUid Target = target;
}
