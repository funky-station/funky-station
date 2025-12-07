// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Heretic.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Heretic.Components;

[Serializable, NetSerializable]
public sealed class HereticRitualMessage : BoundUserInterfaceMessage
{
    public ProtoId<HereticRitualPrototype> ProtoId;

    public HereticRitualMessage(ProtoId<HereticRitualPrototype> protoId)
    {
        ProtoId = protoId;
    }
}

[Serializable, NetSerializable]
public enum HereticRitualRuneUiKey : byte
{
    Key
}
