// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Components;

[RegisterComponent]
public sealed partial class PendingInstabilityMutationComponent : Component
{
    public string MutationId = string.Empty;
    public TimeSpan EndTime;
    public TimeSpan StartTime;
    public bool WarningStart = false;
    public bool WarningHalfway = false;
    public bool Warning10Sec = false;
}
