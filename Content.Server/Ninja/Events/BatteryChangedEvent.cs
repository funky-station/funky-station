// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Ninja.Events;

/// <summary>
/// Raised on the ninja and suit when the suit has its powercell changed.
/// </summary>
[ByRefEvent]
public record struct NinjaBatteryChangedEvent(EntityUid Battery, EntityUid BatteryHolder);
