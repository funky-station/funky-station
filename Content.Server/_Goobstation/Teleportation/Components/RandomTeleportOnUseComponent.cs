// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;

namespace Content.Server.Teleportation;

/// <summary>
/// Entity that will randomly teleport the user when used in hand.
/// </summary>
[RegisterComponent]
public sealed partial class RandomTeleportOnUseComponent : Component
{
    /// <summary>
    /// Whether to consume this item on use; consumes only one if it's a stack
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public bool ConsumeOnUse = true;
}
