// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Database;
using Robust.Shared.Network;

namespace Content.Server.Connection;

/// <summary>
/// Helper functions for working with <see cref="NetUserData"/>.
/// </summary>
public static class UserDataExt
{
    /// <summary>
    /// Get the preferred HWID that should be used for new records related to a player.
    /// </summary>
    /// <remarks>
    /// Players can have zero or more HWIDs, but for logging things like connection logs we generally
    /// only want a single one. This method returns a nullable method.
    /// </remarks>
    public static ImmutableTypedHwid? GetModernHwid(this NetUserData userData)
    {
        return userData.ModernHWIds.Length == 0
            ? null
            : new ImmutableTypedHwid(userData.ModernHWIds[0], HwidType.Modern);
    }
}
