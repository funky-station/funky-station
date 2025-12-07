// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.Weapons.AmmoSelector;

[Serializable, NetSerializable, DataDefinition]
[Prototype("selectableAmmo")]
public sealed partial class SelectableAmmoPrototype : IPrototype, ICloneable
{
    [IdDataField]
    public string ID { get; private set; }

    [DataField(required: true)]
    public SpriteSpecifier Icon;

    [DataField(required: true)]
    public string Desc;

    [DataField(required: true)]
    public EntProtoId ProtoId;

    [DataField]
    public Color? Color;

    public object Clone()
    {
        return new SelectableAmmoPrototype
        {
            ID = ID,
            Icon = Icon,
            Desc = Desc,
            ProtoId = ProtoId,
            Color = Color,
        };
    }
}
