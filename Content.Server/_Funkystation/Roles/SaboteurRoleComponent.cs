// SPDX-FileCopyrightText: 2025 TheHolyAegis <sanderkamphuis719@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;

namespace Content.Server.Roles;

/// <summary>
///     Added to mind role entities to tag that they are a saboteur.
/// </summary>
[RegisterComponent]
public sealed partial class SaboteurRoleComponent : BaseMindRoleComponent
{
}
