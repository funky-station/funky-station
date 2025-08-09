// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Charges.Components;

namespace Content.Server._Funkystation.EmpWeapon.Components;

/// <summary>
/// Causes an item to create a localised EMP burst on an attacked entity
/// </summary>
[RegisterComponent]
public sealed partial class EmpWeaponComponent : Component
{
    /// <summary>
    /// If the item consumes charges to create EMPs.
    /// No EMP will be created if this is true and the item either doesn't have the <see cref="LimitedChargesComponent"/>, or if it has no charges left.
    /// </summary>
    [DataField(required: true)]
    public bool RequiresCharges = false;

    /// <summary>
    /// Range of the EMP in tiles.
    /// </summary>
    [DataField]
    public float EmpRange = 0.5f;

    /// <summary>
    /// Power consumed from batteries by the EMP
    /// </summary>
    [DataField]
    public float EmpConsumption = 5000f;

    /// <summary>
    /// How long the EMP effects last for, in seconds
    /// </summary>
    [DataField]
    public float EmpDuration = 12f;
}
