// SPDX-FileCopyrightText: 2025 jackel234 <colespayde02@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later


namespace Content.Shared._RMC14.Weapons.Ranged;

[ByRefEvent]
public record struct RMCTryAmmoEjectEvent(
    EntityUid User,
    bool Cancelled
);
