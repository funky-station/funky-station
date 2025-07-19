// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.FixedPoint;

namespace Content.Shared._DV.Surgery;

/// <summary>
/// 	Event fired when an object is sterilised for surgery
/// </summary>
[ByRefEvent]
public record struct SurgeryCleanedEvent(FixedPoint2 DirtAmount, int DnaAmount);
