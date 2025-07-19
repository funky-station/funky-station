// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2021 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+SlamBamActionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Voting;

/// <summary>
/// Standard vote types that players can initiate themselves from the escape menu.
/// </summary>
public enum StandardVoteType : byte
{
    /// <summary>
    /// Vote to restart the round.
    /// </summary>
    Restart,

    /// <summary>
    /// Vote to change the game preset for next round.
    /// </summary>
    Preset,

    /// <summary>
    /// Vote to change the map for the next round.
    /// </summary>
    Map,

    /// <summary>
    /// Vote to kick a player.
    /// </summary>
    Votekick
}

/// <summary>
/// Reasons available to initiate a votekick.
/// </summary>
public enum VotekickReasonType : byte
{
    Raiding,
    Cheating,
    Spam
}
