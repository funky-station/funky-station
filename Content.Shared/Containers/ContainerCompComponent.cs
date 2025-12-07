// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Containers;

/// <summary>
/// Applies container changes whenever an entity is inserted into the specified container on this entity.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ContainerCompComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Proto;

    [DataField(required: true)]
    public string Container = string.Empty;
}
