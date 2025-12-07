// SPDX-FileCopyrightText: 2024 Conchelle <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Network;
using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Events;

[Serializable, NetSerializable]
public sealed class AdminInfoEvent(NetUserId userid) : EntityEventArgs
{
    public NetUserId user = userid;
}
