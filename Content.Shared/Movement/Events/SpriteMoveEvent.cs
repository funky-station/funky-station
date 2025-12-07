// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Movement.Events;

/// <summary>
/// Raised on an entity whenever it should change movement sprite
/// </summary>
[ByRefEvent]
public readonly struct SpriteMoveEvent
{
    public readonly bool IsMoving = false;

    public SpriteMoveEvent(bool isMoving)
    {
        IsMoving = isMoving;
    }
}
