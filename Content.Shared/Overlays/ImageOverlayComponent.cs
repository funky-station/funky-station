// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Overlays;

/// <summary>
/// Adds a image based shader when wearing an entity with this component.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class ImageOverlayComponent : Component
{
    /// <summary>
    /// Path to image overlayed on the screen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public ResPath? PathToOverlayImage = default;

    /// <summary>
    /// The additional Color that can be overlayed over whole screen.
    /// </summary>
    [DataField, AutoNetworkedField]
    public Color AdditionalColorOverlay = new(0, 0, 0, 0);
}

