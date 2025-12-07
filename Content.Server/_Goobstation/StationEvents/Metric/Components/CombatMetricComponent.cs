// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.StationEvents.Events;
using Content.Shared.FixedPoint;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Generic;

namespace Content.Server._Goobstation.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(CombatMetricSystem))]
public sealed partial class CombatMetricComponent : Component
{

    /// <summary>
    /// Funky: The rough combat potential of a carp
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 HostileScore = 5.0f;

    /// <summary>
    /// Funky: The rough combat potential of an (unrobust) friendly tider (was 10.0 with Goob)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 FriendlyScore = 2.0f;

    /// <summary>
    ///   Cost per point of medical damage for friendly entities
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 MedicalMultiplier = 0.05f;

    /// <summary>
    ///   Cost for friendlies who are in crit
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 CritScore = 2.0f;

    /// <summary>
    ///   Cost for friendlies who are dead
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 DeadScore = 10.0f;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 maxItemThreat = 15.0f;

    /// <summary>
    ///   ItemThreat - evaluate based on item tags how powerful a player is
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<string, FixedPoint2> ItemThreat =
        new()
        {
            { "Taser", 3.0f },
            { "Sidearm", 3.0f },
            { "Rifle", 5.0f },
            { "HighRiskItem", 4.0f },
            { "CombatKnife", 2.0f },
            { "Knife", 1.5f },
            { "Grenade", 2.0f },
            { "Bomb", 4.0f },
            { "MagazinePistol", 1.0f },
            { "Hacking", 1.0f },
            { "Jetpack", 1.0f },
            { "Armor", 3.0f},
            { "SpecialArmor", 6.0f},
            { "SpecialWeapon", 6.0f},
        };

}
