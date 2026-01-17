// SPDX-FileCopyrightText: 2025 mq <113324899+mqole@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._DV.Polymorph;

/// <summary>
/// Raised directed on an entity before polymorphing it.
/// </summary>
[ByRefEvent]
public record struct BeforePolymorphedEvent();
