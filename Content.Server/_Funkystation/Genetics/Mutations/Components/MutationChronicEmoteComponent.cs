// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

/// <summary>
/// Mutation that causes a periodic emote with a chance to drop held items.
/// </summary>
[RegisterComponent]
public sealed partial class MutationChronicEmoteComponent : Component
{
    /// <summary>
    /// Average time between cough checks in seconds.
    /// </summary>
    [DataField]
    public float Interval = 15.0f;

    /// <summary>
    /// Chance to actually cough when the timer triggers.
    /// </summary>
    [DataField]
    public float EmoteChance = 0.4f;

    /// <summary>
    /// Chance to drop the held item when coughing.
    /// </summary>
    [DataField]
    public float DropChance = 0.25f;

    /// <summary>
    /// The ID of the emote prototype to play (e.g. "Cough").
    /// </summary>
    [DataField(required: true)]
    public string EmoteId = "Cough";

    [ViewVariables]
    public TimeSpan NextCheck;
}
