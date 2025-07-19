// SPDX-FileCopyrightText: 2021 20kdc <asdd2808@gmail.com>
// SPDX-FileCopyrightText: 2022 mirrorcult <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System;

namespace Content.Server.Power.Components
{
    /// <summary>
    ///     Self-recharging battery.
    /// </summary>
    [RegisterComponent]
    public sealed partial class BatterySelfRechargerComponent : Component
    {
        /// <summary>
        /// Does the entity auto recharge?
        /// </summary>
        [DataField] public bool AutoRecharge;

        /// <summary>
        /// At what rate does the entity automatically recharge?
        /// </summary>
        [DataField] public float AutoRechargeRate;

        /// <summary>
        /// Should this entity stop automatically recharging if a charge is used?
        /// </summary>
        [DataField] public bool AutoRechargePause = false;

        /// <summary>
        /// How long should the entity stop automatically recharging if a charge is used?
        /// </summary>
        [DataField] public float AutoRechargePauseTime = 0f;

        /// <summary>
        /// Do not auto recharge if this timestamp has yet to happen, set for the auto recharge pause system.
        /// </summary>
        [DataField] public TimeSpan NextAutoRecharge = TimeSpan.FromSeconds(0f);
    }
}
