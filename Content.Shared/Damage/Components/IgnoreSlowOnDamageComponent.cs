// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Damage.Components;

/// <summary>
/// This is used for an effect that nullifies <see cref="SlowOnDamageComponent"/> and adds an alert.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SlowOnDamageSystem))]
public sealed partial class IgnoreSlowOnDamageComponent : Component;
