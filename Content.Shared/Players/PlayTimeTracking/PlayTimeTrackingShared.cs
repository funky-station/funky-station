// SPDX-FileCopyrightText: 2022 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Veritius <veritiusgaming@gmail.com>
// SPDX-FileCopyrightText: 2024 Repo <47093363+Titian3@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Dataset;

namespace Content.Shared.Players.PlayTimeTracking;

public static class PlayTimeTrackingShared
{
    /// <summary>
    /// The prototype ID of the play time tracker that represents overall playtime, i.e. not tied to any one role.
    /// </summary>
    [ValidatePrototypeId<PlayTimeTrackerPrototype>]
    public const string TrackerOverall = "Overall";

    /// <summary>
    /// The prototype ID of the play time tracker that represents admin time, when a player is in game as admin.
    /// </summary>
    [ValidatePrototypeId<PlayTimeTrackerPrototype>]
    public const string TrackerAdmin = "Admin";
}
