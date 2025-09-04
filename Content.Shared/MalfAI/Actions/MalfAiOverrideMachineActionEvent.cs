// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.MalfAI.Actions;

/// <summary>
/// Action event for the Malf AI Override Machine ability.
/// This must be a world-targeted action to receive a map Target from the client.
/// </summary>
public sealed partial class MalfAiOverrideMachineActionEvent : WorldTargetActionEvent
{
}
