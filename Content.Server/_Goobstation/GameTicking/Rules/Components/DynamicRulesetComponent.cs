// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.GameTicking.Rules.Components;

[RegisterComponent]
public sealed partial class DynamicRulesetComponent : Component
{
    /// <summary>
    ///     Localized name that will be shown at EORG
    /// </summary>
    [DataField] public string NameLoc;

    /// <summary>
    ///     Required minimum amount of valids to start the gamerule
    /// </summary>
    [DataField] public float RequiredCandidates = 1f;

    /// <summary>
    ///     Gamerule weight
    /// </summary>
    [DataField] public float Weight = 5f;

    /// <summary>
    ///     Initial cost for the first antagonist
    /// </summary>
    [DataField] public float Cost = 8f;

    /// <summary>
    ///     Cost to spawn in more antagonists
    /// </summary>
    [DataField] public float ScalingCost = 1f;

    /// <summary>
    ///     High impact indicated that all other high impact rulesets are to be removed.
    /// </summary>
    [DataField] public bool HighImpact = false;
}
