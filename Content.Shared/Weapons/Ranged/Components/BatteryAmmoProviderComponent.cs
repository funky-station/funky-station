// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 LordEclipse <106132477+LordEclipse@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Weapons.Ranged.Components;

public abstract partial class BatteryAmmoProviderComponent : AmmoProviderComponent
{
    /// <summary>
    /// How much battery it costs to fire once.
    /// </summary>
    [DataField("fireCost")] // Shitmed Change
    public float FireCost = 100;

    // Batteries aren't predicted which means we need to track the battery and manually count it ourselves woo!

    [ViewVariables(VVAccess.ReadWrite)]
    public int Shots;

    [ViewVariables(VVAccess.ReadWrite)]
    public int Capacity;
}