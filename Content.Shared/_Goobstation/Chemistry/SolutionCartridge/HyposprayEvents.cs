// SPDX-FileCopyrightText: 2025 IronDragoon <8961391+IronDragoon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 IronDragoon <you@example.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Tag;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Chemistry.SolutionCartridge;

/// <summary>
/// Raised on a hypospray when it successfully injects.
/// </summary>
[ByRefEvent]
public record struct AfterHyposprayInjectsEvent()
{
    /// <summary>
    /// Entity that used the hypospray.
    /// </summary>
    public EntityUid User;

    /// <summary>
    /// Entity that was injected.
    /// </summary>
    public EntityUid Target;
}
