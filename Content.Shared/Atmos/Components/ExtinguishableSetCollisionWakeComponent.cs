// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// Makes entities with extinguishing behavior automatically enable/disable <see cref="CollisionWakeComponent"/>,
/// so they can be extinguished with fire extinguishers.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
public sealed partial class ExtinguishableSetCollisionWakeComponent : Component;
