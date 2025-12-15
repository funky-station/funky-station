// SPDX-FileCopyrightText: 2025 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Light.Components;

/// <summary>
/// Assumes the entire attached grid is rooved.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ImplicitRoofComponent : Component
{
    [DataField, AutoNetworkedField]
    public Color Color = Color.Black;
}
