// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server._EinsteinEngines.Power.Components;

[RegisterComponent]
public sealed partial class BatteryDrinkerComponent : Component
{
    /// <summary>
    ///     Is this drinker allowed to drink batteries not tagged as <see cref="BatteryDrinkSource"/>?
    /// </summary>
    [DataField]
    public bool DrinkAll;

    /// <summary>
    ///     How long it takes to drink from a battery, in seconds.
    ///     Is multiplied by the source.
    /// </summary>
    [DataField]
    public float DrinkSpeed = 1.5f;

    /// <summary>
    ///     The multiplier for the amount of power to attempt to drink.
    ///     Default amount is 1000
    /// </summary>
    [DataField]
    public float DrinkMultiplier = 5f;

    /// <summary>
    ///     The multiplier for how long it takes to drink a non-source battery, if <see cref="DrinkAll"/> is true.
    /// </summary>
    [DataField]
    public float DrinkAllMultiplier = 2.5f;
}
