// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Construction.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Construction.Components;

/// <summary>
/// Will not allow anchoring if there is an anchored item in the same tile that fails the <see cref="EntityWhitelist"/>.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(BlockAnchorOnSystem))]
public sealed partial class BlockAnchorOnComponent : Component
{
    /// <summary>
    /// If not null, entities that match this whitelist are allowed.
    /// </summary>
    [DataField]
    public EntityWhitelist? Whitelist;

    /// <summary>
    /// If not null, entities that match this blacklist are not allowed.
    /// </summary>
    [DataField]
    public EntityWhitelist? Blacklist;
}
