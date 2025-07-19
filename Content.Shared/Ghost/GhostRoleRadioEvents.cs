// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Ghost.Roles;

[Serializable, NetSerializable]
public sealed class GhostRoleRadioMessage : BoundUserInterfaceMessage
{
    public ProtoId<GhostRolePrototype> ProtoId;

    public GhostRoleRadioMessage(ProtoId<GhostRolePrototype> protoId)
    {
        ProtoId = protoId;
    }
}

[Serializable, NetSerializable]
public enum GhostRoleRadioUiKey : byte
{
    Key
}
