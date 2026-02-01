// SPDX-FileCopyrightText: 2025 Amethyst <52829582+jackel234@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 TrixxedHeart <46364955+TrixxedBit@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 maelines <amae.tones@gmail.com>
// SPDX-FileCopyrightText: 2025 maelines <genovedd.almn@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._CD.Traits;

/// <summary>
/// Set players' blood to coolant, and is used to notify them of ion storms
/// </summary>
[RegisterComponent, Access(typeof(SynthSystem))]
public sealed partial class SynthComponent : Component
{
    /// <summary>
    /// The chance that the synth is alerted of an ion storm
    /// </summary>
    [DataField]
    public float AlertChance = 0.0f; // Funky change, people cannot behave.
}
