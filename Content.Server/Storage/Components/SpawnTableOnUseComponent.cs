// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Storage.EntitySystems;
using Content.Shared.EntityTable.EntitySelectors;

namespace Content.Server.Storage.Components;

/// <summary>
/// Spawns items from an entity table when used in hand.
/// </summary>
[RegisterComponent, Access(typeof(SpawnTableOnUseSystem))]
public sealed partial class SpawnTableOnUseComponent : Component
{
    /// <summary>
    /// The entity table to select entities from.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;
}
