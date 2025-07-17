// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Procedural.PostGeneration;

/// <summary>
/// Places tiles / entities onto room entrances.
/// </summary>
/// <remarks>
/// DungeonData keys are:
/// - Entrance
/// - FallbackTile
/// </remarks>
public sealed partial class RoomEntranceDunGen : IDunGenLayer;
