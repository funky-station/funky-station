// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server._Goobstation.WeaponRandomExplode;

[RegisterComponent]
public sealed partial class WeaponRandomExplodeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float explosionChance;

    /// <summary>
    /// if not filled - the explosion force will be 1.
    /// if filled - the explosion force will be the current charge multiplied by this.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float multiplyByCharge;

    /// <summary> 
    /// decreases the self damage and explosion radius   #funkystation
    /// </summary>
    [DataField, AutoNetworkedField]
    public float? reduction;

    /// <summary>
    /// deletes the gun after the explosion if this is true   #funkystation
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool destroyGun;
}
