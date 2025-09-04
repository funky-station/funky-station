// SPDX-FileCopyrightText: 2025 YourName
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;

namespace Content.Shared.MalfAI.Actions;

/// <summary>
/// Action event for the Malf AI Lockdown Grid ability.
/// </summary>
public sealed partial class MalfAiLockdownGridActionEvent : InstantActionEvent
{
    /// <summary>
    /// Duration of the lockdown in seconds.
    /// </summary>
    public float Duration = 30f;
}
