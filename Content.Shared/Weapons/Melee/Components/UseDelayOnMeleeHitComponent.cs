// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Components;

/// <summary>
///     Activates UseDelay when a Melee Weapon is used to hit something.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(UseDelayOnMeleeHitSystem))]
public sealed partial class UseDelayOnMeleeHitComponent : Component
{

}
