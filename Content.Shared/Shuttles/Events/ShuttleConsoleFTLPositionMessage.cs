// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on the client when it wishes to travel somewhere.
/// </summary>
[Serializable, NetSerializable]
public sealed class ShuttleConsoleFTLPositionMessage : BoundUserInterfaceMessage
{
    public MapCoordinates Coordinates;
    public Angle Angle;
}
