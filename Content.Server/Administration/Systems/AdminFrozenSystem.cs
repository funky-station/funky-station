// SPDX-FileCopyrightText: 2024 no <165581243+pissdemon@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;

namespace Content.Server.Administration.Systems;

public sealed class AdminFrozenSystem : SharedAdminFrozenSystem
{
    /// <summary>
    /// Freezes and mutes the given entity.
    /// </summary>
    public void FreezeAndMute(EntityUid uid)
    {
        var comp = EnsureComp<AdminFrozenComponent>(uid);
        comp.Muted = true;
        Dirty(uid, comp);
    }
}
