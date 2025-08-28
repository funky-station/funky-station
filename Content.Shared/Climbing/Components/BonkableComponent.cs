// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Ilya Chvilyov <90278813+Telyonok@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Climbing.Components;

/// <summary>
///     Makes entity do damage and stun entities with ClumsyComponent
///     upon DragDrop or Climb interactions.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BonkableComponent : Component
{
    /// <summary>
    ///     How long to stun players on bonk, in seconds.
    /// </summary>
    [DataField]
    public TimeSpan BonkTime = TimeSpan.FromSeconds(2);

    /// <summary>
    ///     How much damage to apply on bonk.
    /// </summary>
    [DataField]
    public DamageSpecifier? BonkDamage;
}
