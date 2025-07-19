// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.StationEvents.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(BlobSpawnRule))]
public sealed partial class BlobSpawnRuleComponent : Component
{
    [DataField("carrierBlobProtos", required: true, customTypeSerializer: typeof(PrototypeIdListSerializer<EntityPrototype>)), ViewVariables(VVAccess.ReadWrite)]
    public List<string> CarrierBlobProtos = new()
    {
        "SpawnPointGhostBlobRat"
    };

    [ViewVariables(VVAccess.ReadOnly), DataField("playersPerCarrierBlob")]
    public int PlayersPerCarrierBlob = 30;

    [ViewVariables(VVAccess.ReadOnly), DataField("maxCarrierBlob")]
    public int MaxCarrierBlob = 2;
}
