// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Chemistry.Events;

/// <summary>
/// Raised directed on an entity when it embeds in another entity.
/// </summary>
[ByRefEvent]
public readonly record struct InjectOverTimeEvent(EntityUid embeddedIntoUid)
{
    /// <summary>
    /// Entity that is embedded in.
    /// </summary>
    public readonly EntityUid EmbeddedIntoUid = embeddedIntoUid;
}
