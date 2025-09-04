// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.MalfAI.Actions;

/// <summary>
/// Action event for the Malf AI Overload Machine ability.
/// This must be a world-targeted action to receive a map Target from the client.
/// </summary>
public sealed partial class MalfAiOverloadMachineActionEvent : WorldTargetActionEvent
{
}
