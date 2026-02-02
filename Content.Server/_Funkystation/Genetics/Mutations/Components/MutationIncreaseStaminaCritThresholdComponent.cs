// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Gives the user a buff to stamina
/// </summary>
[RegisterComponent]
public sealed partial class MutationIncreaseStaminaCritThresholdComponent : Component
{
    [DataField]
    public float ThresholdBonus = 30f;  // +30 = 130 threshold
}
