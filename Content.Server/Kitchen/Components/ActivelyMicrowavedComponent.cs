// SPDX-FileCopyrightText: 2024 deathride58 <deathride58@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Kitchen.Components;

/// <summary>
/// Attached to an object that's actively being microwaved
/// </summary>
[RegisterComponent]
public sealed partial class ActivelyMicrowavedComponent : Component
{
    /// <summary>
    /// The microwave this entity is actively being microwaved by.
    /// </summary>
    [DataField]
    public EntityUid? Microwave;
}
