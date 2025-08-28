// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Simon <63975668+Simyon264@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Threading.Tasks;
using Robust.Server.Player;
using Robust.Shared.Network;

namespace Content.Server.Connection.Whitelist.Conditions;

/// <summary>
/// Condition that matches if the player count is within a certain range.
/// </summary>
public sealed partial class ConditionPlayerCount : WhitelistCondition
{
    [DataField]
    public int MinimumPlayers  = 0;
    [DataField]
    public int MaximumPlayers = int.MaxValue;
}
