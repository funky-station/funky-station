// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Component for the Malfunctioning AI game rule.
/// Handles the setup and management of Malf AI antagonist rounds.
/// </summary>
[RegisterComponent, EntityCategory("GameRules")]
public sealed partial class MalfAiRuleComponent : Component;
