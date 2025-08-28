// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Atmos;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent]
public sealed partial class CosmicSpireComponent : Component
{

    [DataField]
    public bool Enabled;

    [DataField]
    public float DrainRate = 550;

    [DataField]
    public float DrainThreshHold = 2500;

    [DataField]
    public HashSet<Gas> DrainGases =
    [
        Gas.Oxygen,
        Gas.Nitrogen,
        Gas.CarbonDioxide,
        Gas.WaterVapor,
        Gas.Ammonia,
        Gas.NitrousOxide,
    ];

    [DataField]
    public GasMixture Storage = new();

    [DataField]
    public EntProtoId EntropyMote = "MaterialCosmicCultEntropy1";

    [DataField]
    public EntProtoId SpawnVFX = "CosmicGenericVFX";
}

[Serializable, NetSerializable]
public enum SpireVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum SpireStatus : byte
{
    Off,
    On,
}
