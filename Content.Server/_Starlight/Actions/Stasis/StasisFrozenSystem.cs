// SPDX-FileCopyrightText: 2025 SigmaTheDragon <162711378+SigmaTheDragon@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Starlight.Actions.Stasis;

namespace Content.Server._Starlight.Actions.Stasis;

public sealed class StasisFrozenSystem : SharedStasisFrozenSystem
{
    /// <summary>
    /// Freezes and mutes the given entity.
	/// # Starlight, under MIT License
    /// </summary>
    public void FreezeAndMute(EntityUid uid)
    {
        var comp = EnsureComp<StasisFrozenComponent>(uid);
        comp.Muted = false;
        Dirty(uid, comp);
    }
}
