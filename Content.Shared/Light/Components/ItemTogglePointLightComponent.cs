// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Centronias <me@centronias.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.GameStates;
using Content.Shared.Toggleable;

namespace Content.Shared.Light.Components;

/// <summary>
/// Makes <see cref="ItemToggledEvent"/> enable and disable point lights on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ItemTogglePointLightComponent : Component
{
    /// <summary>
    /// When true, causes the color specified in <see cref="ToggleableVisuals.Color"/>
    /// be used to modulate the color of lights on this entity.
    /// </summary>
    [DataField] public bool ToggleableVisualsColorModulatesLights = false;
}
