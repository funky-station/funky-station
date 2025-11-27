// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Cloning.Events;

/// <summary>
///    Raised before a mob is cloned. Cancel to prevent cloning.
///    This is raised on the original mob.
/// </summary>
[ByRefEvent]
public record struct CloningAttemptEvent(CloningSettingsPrototype Settings, bool Cancelled = false);

/// <summary>
///    Raised after a new mob was spawned when cloning a humanoid.
///    This is raised on the original mob.
/// </summary>
[ByRefEvent]
public record struct CloningEvent(CloningSettingsPrototype Settings, EntityUid CloneUid);

/// <summary>
///    Raised after a new item was spawned when cloning an item.
///    This is raised on the original item.
/// </summary>
[ByRefEvent]
public record struct CloningItemEvent(EntityUid CloneUid);
