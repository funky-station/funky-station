// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Vasilis <vasilis@pikachu.systems>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Players.PlayTimeTracking;

namespace Content.Shared.Roles;

/// <summary>
///     Event raised on a mind entity to get all roles that a player has.
/// </summary>
/// <param name="Roles">The list of roles on the player.</param>
[ByRefEvent]
public readonly record struct MindGetAllRoleInfoEvent(List<RoleInfo> Roles);

/// <summary>
///     Returned by <see cref="MindGetAllRolesEvent"/> to give some information about a player's role.
/// </summary>
/// <param name="Component">Role component associated with the mind entity id.</param>
/// <param name="Name">Name of the role.</param>
/// <param name="Antagonist">Whether or not this role makes this player an antagonist.</param>
/// <param name="PlayTimeTrackerId">The <see cref="PlayTimeTrackerPrototype"/> id associated with the role.</param>
/// <param name="Prototype">The prototype ID of the role</param>
public readonly record struct RoleInfo(string Name, bool Antagonist, string? PlayTimeTrackerId, string Prototype);
