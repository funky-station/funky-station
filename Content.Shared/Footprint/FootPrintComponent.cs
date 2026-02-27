// SPDX-FileCopyrightText: 2025 VMSolidus <evilexecutive@gmail.com>
// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Footprint;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootprintComponent : Component
{
    [AutoNetworkedField, ViewVariables]
    public List<Footprint> Footprints = new();
}

[Serializable, NetSerializable]
public readonly record struct Footprint(Vector2 Offset, Angle Rotation, Color Color, string State);
