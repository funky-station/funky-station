// SPDX-FileCopyrightText: 2023 Stray-Pyramid <Pharaohofnile@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Inventory;

namespace Content.Server.Storage.Components;

/// <summary>
/// Applies an ongoing pickup area around the attached entity.
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class MagnetPickupComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("nextScan")]
    [AutoPausedField]
    public TimeSpan NextScan = TimeSpan.Zero;

    /// <summary>
    /// What container slot the magnet needs to be in to work.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("slotFlags")]
    public SlotFlags SlotFlags = SlotFlags.BELT;

    [ViewVariables(VVAccess.ReadWrite), DataField("range")]
    public float Range = 1f;
}
