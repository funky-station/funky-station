// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Shared.Procedural.DungeonLayers;


/// <summary>
/// Spawns entities inside of the dungeon randomly.
/// </summary>
public sealed partial class EntityTableDunGen : IDunGenLayer
{
    // Counts separate to config to avoid some duplication.

    [DataField]
    public int MinCount = 1;

    [DataField]
    public int MaxCount = 1;

    [DataField(required: true)]
    public EntityTableSelector Table;
}
