// SPDX-FileCopyrightText: 2024 blueDev2 <89804215+blueDev2@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Whitelist;
using Robust.Shared.GameStates;

namespace Content.Shared.Sound.Components;

/// <summary>
/// Whenever this item is used upon by an entity, with a tag or component within a whitelist, in the hand of a user, play a sound
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class EmitSoundOnInteractUsingComponent : BaseEmitSoundComponent
{
    /// <summary>
    /// The <see cref="EntityWhitelist"/> for the entities that can use this item.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = new();
}
