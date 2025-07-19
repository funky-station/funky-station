// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Medical.Surgery;

[Serializable, NetSerializable]
public sealed class SurgeryUiRefreshEvent : EntityEventArgs
{
    public NetEntity Uid { get; }

    public SurgeryUiRefreshEvent(NetEntity uid)
    {
        Uid = uid;
    }
}
