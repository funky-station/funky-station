// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.GameTicking.Components;

/// <summary>
///     Added to game rules before <see cref="GameRuleEndedEvent"/>.
///     Mutually exclusive with <seealso cref="ActiveGameRuleComponent"/>.
/// </summary>
[RegisterComponent]
public sealed partial class EndedGameRuleComponent : Component;
