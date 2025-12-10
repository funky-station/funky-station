// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
/// Tries to manage a single sprite prefix based on multiple independent states,
/// like a helmet visor and a headlamp. I don't know what I'm doing.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class MultiVisualStateComponent : Component
{
    /// <summary>
    /// The prefix to use when the visor is UP and the light is OFF
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PrefixVisorOffLightOff;

    /// <summary>
    /// The prefix to use when the visor is UP and the light is ON
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PrefixVisorOffLightOn;

    /// <summary>
    /// The prefix to use when the visor is DOWN and the light is OFF
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PrefixVisorOnLightOff;

    /// <summary>
    /// The prefix to use when the visor is DOWN and the light is ON
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? PrefixVisorOnLightOn;

    /// <summary>
    /// Current live state of the visor. True = Down/Active
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool VisorState;

    /// <summary>
    /// Current live state of the light. True = On
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool LightState;
}
