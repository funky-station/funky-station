// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Adds to a mind role ent to tag they're a Survivor
/// </summary>
[RegisterComponent]
public sealed partial class SurvivorRoleComponent : BaseMindRoleComponent;
