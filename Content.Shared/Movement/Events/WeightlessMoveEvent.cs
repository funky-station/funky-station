// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity to check if it can move while weightless.
/// </summary>
[ByRefEvent]
public record struct CanWeightlessMoveEvent(EntityUid Uid)
{
    public bool CanMove = false;
}
