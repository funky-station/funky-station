// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.SubFloor;

/// <summary>
/// For tile-like entities, such as catwalk and carpets, to reveal subfloor entities when on the same tile and when
/// using a t-ray scanner.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TrayScanRevealComponent : Component;
