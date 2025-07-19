// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Paper;

namespace Content.Shared._DV.Paper;

/// <summary>
/// Raised on the pen when trying to sign a paper.
/// If it's cancelled the signature isn't made.
/// </summary>
[ByRefEvent]
public record struct SignAttemptEvent(Entity<PaperComponent> Paper, EntityUid User, EntityUid Pen, bool Cancelled = false);
