// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 gluesniffler <159397573+gluesniffler@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Shitmed.Autodoc.Systems;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Autodoc.Components;

/// <summary>
/// Creates a list of hands and spawns items to fill them.
/// </summary>
[RegisterComponent, Access(typeof(HandsFillSystem))]
public sealed partial class HandsFillComponent : Component
{
    /// <summary>
    /// The name of each hand and the item to fill it with, if any.
    /// </summary>
    [DataField(required: true)]
    public Dictionary<string, EntProtoId?> Hands = new();
}
