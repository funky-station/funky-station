// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationBloodRegenerationComponent : Component
{
    /// <summary>
    ///     How much blood (in units) to regenerate per second.
    /// </summary>
    [DataField("regenRatePerSecond")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float RegenRatePerSecond = 2.0f;

    /// <summary>
    ///     The target blood level percentage (0.0 to 1.0) to passively regenerate up to.
    ///     e.g. 0.8 = regenerate until blood is at 80% of max.
    /// </summary>
    [DataField("targetPercentage")]
    [ViewVariables(VVAccess.ReadWrite)]
    public float TargetPercentage = 1.0f;
}
