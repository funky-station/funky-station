// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;

namespace Content.Server._EinsteinEngines.Silicon.Charge;

[RegisterComponent]
public sealed partial class BatteryDrinkerSourceComponent : Component
{
    /// <summary>
    ///     The max amount of power this source can provide in one sip.
    ///     No limit if null.
    /// </summary>
    [DataField]
    public int? MaxAmount = null;

    /// <summary>
    ///     The multiplier for the drink speed.
    /// </summary>
    [DataField]
    public float DrinkSpeedMulti = 1f;

    /// <summary>
    ///     The sound to play when the battery gets drunk from.
    /// </summary>
    [DataField]
    public SoundSpecifier? DrinkSound = new SoundCollectionSpecifier("sparks");
}
