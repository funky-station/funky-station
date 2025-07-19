// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Access.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Access.Components;

/// <summary>
/// Toggles an access provider with <c>ItemToggle</c>.
/// Requires <see cref="AccessComponent"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(AccessToggleSystem))]
public sealed partial class AccessToggleComponent : Component;
