// SPDX-FileCopyrightText: 2023 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Animates a point light's rotation while enabled.
/// All animation is done in the client system.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
[Access(typeof(SharedRotatingLightSystem))]
public sealed partial class RotatingLightComponent : Component
{
    /// <summary>
    /// Speed to rotate at, in degrees per second
    /// </summary>
    [DataField("speed")]
    public float Speed = 90f;

    [ViewVariables, AutoNetworkedField]
    public bool Enabled = true;
}
