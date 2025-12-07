// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._Shitmed.Targeting;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Targeting.Events;

[Serializable, NetSerializable]
public sealed class TargetChangeEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public TargetBodyPart BodyPart { get; }
    public TargetChangeEvent(NetEntity uid, TargetBodyPart bodyPart)
    {
        Uid = uid;
        BodyPart = bodyPart;
    }
}

[Serializable, NetSerializable]
public sealed class TargetIntegrityChangeEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public bool RefreshUi { get; }
    public TargetIntegrityChangeEvent(NetEntity uid, bool refreshUi = true)
    {
        Uid = uid;
        RefreshUi = refreshUi;
    }
}
