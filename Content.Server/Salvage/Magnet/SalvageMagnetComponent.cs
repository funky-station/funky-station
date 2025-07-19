// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Server.Salvage.Magnet;

[RegisterComponent]
public sealed partial class SalvageMagnetComponent : Component
{
    /// <summary>
    /// The max distance at which the magnet will pull in wrecks.
    /// Scales from 50% to 100%.
    /// </summary>
    [DataField]
    public float MagnetSpawnDistance = 64f;

    /// <summary>
    /// How far offset to either side will the magnet wreck spawn.
    /// </summary>
    [DataField]
    public float LateralOffset = 16f;
}
