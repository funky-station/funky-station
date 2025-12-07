// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Client.Movement.Systems;
using Content.Shared.Movement.Components;

namespace Content.Client.Movement.Components;

[RegisterComponent]
public sealed partial class EyeCursorOffsetComponent : SharedEyeCursorOffsetComponent
{
    /// <summary>
    /// The location the offset will attempt to pan towards; based on the cursor's position in the game window.
    /// </summary>
    public Vector2 TargetPosition = Vector2.Zero;

    /// <summary>
    /// The current positional offset being applied. Used to enable gradual panning.
    /// </summary>
    public Vector2 CurrentPosition = Vector2.Zero;
}
