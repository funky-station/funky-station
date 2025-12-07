// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared.Shuttles.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FtlVisualizerComponent : Component
{
    /// <summary>
    /// Clientside time tracker for the animation.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public float Elapsed;

    [DataField(required: true)]
    public SpriteSpecifier.Rsi Sprite;

    /// <summary>
    /// Target grid to pull FTL visualization from.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Grid;
}
