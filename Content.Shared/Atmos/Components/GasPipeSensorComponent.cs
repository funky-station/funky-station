// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Atmos.Components;

/// <summary>
/// Entities with component will be queried against for their
/// atmos monitoring data on atmos monitoring consoles
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GasPipeSensorComponent : Component;
