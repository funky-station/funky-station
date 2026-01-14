// SPDX-FileCopyrightText: 2025 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.NPC.Queries.Considerations;

/// <summary>
/// Returns if the target is below a certain temperature.
/// </summary>
public sealed partial class TargetLowTempCon : UtilityConsideration
{
    /// <summary>
    /// The minimum temperature they must be.
    /// </summary>
    [DataField]
    public float MinTemp;
}

