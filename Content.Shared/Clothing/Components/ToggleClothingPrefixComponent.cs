// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Handles the changes to ClothingComponent.EquippedPrefix when toggled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ToggleClothingPrefixComponent : Component
{
    /// <summary>
    /// Clothing's EquippedPrefix when activated.
    /// </summary>
    [DataField]
    public string? PrefixOn = "on";

    /// <summary>
    /// Clothing's EquippedPrefix when deactivated.
    /// </summary>
    [DataField]
    public string? PrefixOff;
}
