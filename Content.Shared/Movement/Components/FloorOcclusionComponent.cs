// SPDX-FileCopyrightText: 2024 Aidenkrz <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Applies an occlusion shader to this entity if it's colliding with a <see cref="FloorOccluderComponent"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FloorOcclusionComponent : Component
{
    [ViewVariables]
    public bool Enabled => Colliding.Count > 0;

    [DataField, AutoNetworkedField]
    public List<EntityUid> Colliding = new();
}
