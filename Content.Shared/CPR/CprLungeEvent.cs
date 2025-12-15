// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared.Cpr;

/// <summary>
/// Data for CPR animations
/// </summary>
[Serializable, NetSerializable]
public sealed partial class CprLungeEvent : EntityEventArgs
{
    public NetEntity Ent;

    public CprLungeEvent(NetEntity entity)
    {
        Ent = entity;
    }
}
