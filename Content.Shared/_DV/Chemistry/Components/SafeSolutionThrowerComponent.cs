// SPDX-FileCopyrightText: 2025 0vrseer <iov3rseeri@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._DV.Chemistry.Components;

/// <summary>
/// Allows an entity to safely throw solutions without spilling them.
/// Works when added either directly to an entity or to a piece of clothing worn by that entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SafeSolutionThrowerComponent : Component;
