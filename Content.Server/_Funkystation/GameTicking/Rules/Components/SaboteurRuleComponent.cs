// SPDX-FileCopyrightText: 2025 TheHolyAegis <sanderkamphuis719@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.GameTicking.Rules.Components;

/// <summary>
/// Stores data for <see cref="SaboteurRuleSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(SaboteurRuleSystem))]
public sealed partial class SaboteurRuleComponent : Component;
