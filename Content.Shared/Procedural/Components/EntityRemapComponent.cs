// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;

namespace Content.Shared.Procedural.Components;

/// <summary>
/// Indicates this entity prototype should be re-mapped to another
/// </summary>
[RegisterComponent]
public sealed partial class EntityRemapComponent : Component
{
    [DataField(required: true)]
    public Dictionary<EntProtoId, EntProtoId> Mask = new();
}
