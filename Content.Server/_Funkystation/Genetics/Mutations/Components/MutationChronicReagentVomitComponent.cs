// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class MutationChronicReagentVomitComponent : Component
{
    /// <summary>
    /// Reagent to vomit into the puddle.
    /// </summary>
    [DataField(required: true)]
    public string Reagent = default!;

    /// <summary>
    /// Minimum amount of reagent per vomit.
    /// </summary>
    [DataField]
    public int MinAmount = 5;

    /// <summary>
    /// Maximum amount of reagent per vomit.
    /// </summary>
    [DataField]
    public int MaxAmount = 10;

    /// <summary>
    /// Minimum time between vomits (seconds).
    /// </summary>
    [DataField]
    public float MinInterval = 120f;

    /// <summary>
    /// Maximum time between vomits (seconds).
    /// </summary>
    [DataField]
    public float MaxInterval = 300f;

    /// <summary>
    /// Chance that a vomit occurs when the timer expires (0-1).
    /// </summary>
    [DataField]
    public float Chance = 0.6f;

    [DataField]
    public TimeSpan NextVomitTime;
}
