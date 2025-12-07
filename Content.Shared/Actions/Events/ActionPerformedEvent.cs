// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Actions.Events;

/// <summary>
///     Raised on the action entity when it is used and <see cref="BaseActionEvent.Handled"/>.
/// </summary>
/// <param name="Performer">The entity that performed this action.</param>
[ByRefEvent]
public readonly record struct ActionPerformedEvent(EntityUid Performer);
