// SPDX-FileCopyrightText: 2024 Julian Giebel <juliangiebel@live.de>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Power;

/// <summary>
/// Component for a powered machine that slowly powers on and off over a period of time.
/// </summary>
public abstract partial class SharedPowerChargeComponent : Component
{
    /// <summary>
    /// The title used for the default charged machine window if used
    /// </summary>
    [DataField]
    public LocId WindowTitle { get; set; } = string.Empty;

}
