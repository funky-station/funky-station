// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;

namespace Content.Client.Animations;

/// <summary>
/// Entities with this component tracks the user's world position every frame.
/// </summary>
[RegisterComponent]
public sealed partial class TrackUserComponent : Component
{
    public EntityUid? User;

    /// <summary>
    /// Offset in the direction of the entity's rotation.
    /// </summary>
    public Vector2 Offset = Vector2.Zero;
}
