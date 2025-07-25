// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server._Goobstation.ChronoLegionnaire.Components;

/// <summary>
/// Marks gun entity that will return in owner hand or belt when thrown
/// </summary>
[RegisterComponent]
public sealed partial class StasisGunComponent : Component
{
    /// <summary>
    /// Slot which weapon will attempt to return
    /// </summary>
    [DataField]
    public string ReturningSlot = "belt";
}
