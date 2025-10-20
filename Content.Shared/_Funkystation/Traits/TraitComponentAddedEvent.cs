// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Player;

namespace Content.Shared.GameTicking;

/// <summary>
///
/// </summary>
[PublicAPI]
public sealed class TraitComponentAddedEvent : EntityEventArgs
{
    public TraitPrototype Trait { get; }
    public PlayerSpawnCompleteEvent SpawnCompleteEvent { get; }


    public TraitComponentAddedEvent(
        TraitPrototype trait,
        PlayerSpawnCompleteEvent spawnCompleteEvent)
    {
        Trait = trait;
        SpawnCompleteEvent = spawnCompleteEvent;
    }
}
