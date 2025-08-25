// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cloning;
using Robust.Shared.Prototypes;

namespace Content.Server.Cloning.Components;

/// <summary>
///     This is added to a marker entity in order to spawn a clone of a random player.
/// </summary>
[RegisterComponent, EntityCategory("Spawner")]
public sealed partial class RandomCloneSpawnerComponent : Component
{
    /// <summary>
    ///     Cloning settings to be used.
    /// </summary>
    [DataField]
    public ProtoId<CloningSettingsPrototype> Settings = "BaseClone";
}
