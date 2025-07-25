// SPDX-FileCopyrightText: 2025 McBosserson <148172569+McBosserson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Power.EntitySystems;
using Content.Shared.Power;

namespace Content.Server.Power.Components;

/// <summary>
/// Necessary component for battery management UI for SMES/substations.
/// </summary>
/// <seealso cref="BatteryUiKey.Key"/>
/// <seealso cref="BatteryInterfaceSystem"/>
[RegisterComponent]
public sealed partial class BatteryInterfaceComponent : Component
{
    /// <summary>
    /// The maximum charge rate users can configure through the UI.
    /// </summary>
    [DataField]
    public float MaxChargeRate;

    /// <summary>
    /// The minimum charge rate users can configure through the UI.
    /// </summary>
    [DataField]
    public float MinChargeRate;

    /// <summary>
    /// The maximum discharge rate users can configure through the UI.
    /// </summary>
    [DataField]
    public float MaxSupply;

    /// <summary>
    /// The minimum discharge rate users can configure through the UI.
    /// </summary>
    [DataField]
    public float MinSupply;
}
