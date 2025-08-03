// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Projectiles;

/// <summary>
/// Stores a list of all stuck entities to release when this entity is deleted.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmbeddedContainerComponent : Component
{
    [DataField, AutoNetworkedField]
    public HashSet<EntityUid> EmbeddedObjects = new();
}
