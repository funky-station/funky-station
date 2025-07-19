// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Item.ItemToggle.Components;

/// <summary>
/// Handles the changes to ItemComponent.HeldPrefix when toggled.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemTogglePrefixComponent : Component
{
    /// <summary>
    /// Item's HeldPrefix when activated.
    /// </summary>
    [DataField]
    public string? PrefixOn = "on";

    /// <summary>
    /// Item's HeldPrefix when deactivated.
    /// </summary>
    [DataField]
    public string? PrefixOff;
}
