// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Mutation component that has a chance to apply toxin damage at regular intervals.
/// </summary>
[RegisterComponent]
public sealed partial class MutationBloodToxificationComponent : Component
{
    /// <summary>
    /// Interval between toxin damage checks, in seconds.
    /// </summary>
    [DataField]
    public float Interval = 4.0f;

    /// <summary>
    /// Probability of applying toxin damage each interval.
    /// </summary>
    [DataField]
    public float Chance = 0.25f;

    /// <summary>
    /// Amount of toxin damage to apply if the chance succeeds.
    /// </summary>
    [DataField]
    public float ToxinAmount = 1.0f;

    [ViewVariables]
    public TimeSpan NextTick;
}
