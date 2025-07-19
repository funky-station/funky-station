// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the SurvivorRuleSystem. Game rule that turns everyone into a survivor and gives them the objective to escape centcom alive.
/// Started by Wizard Summon Guns/Magic spells.
/// </summary>
[RegisterComponent, Access(typeof(SurvivorRuleSystem))]
public sealed partial class SurvivorRuleComponent : Component;
