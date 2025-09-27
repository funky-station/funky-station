// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Shared._Shitmed.Medical.Surgery.Effects.Complete;

/// <summary>
///     Raised on the entity that received the surgery.
/// </summary>
[ByRefEvent]
public record struct SurgeryCompletedEvent;