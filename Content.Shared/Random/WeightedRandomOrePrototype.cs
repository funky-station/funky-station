// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Vordenburg <114301317+Vordenburg@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Mining;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;

namespace Content.Shared.Random;

/// <summary>
/// Linter-friendly version of weightedRandom for Ore prototypes.
/// </summary>
[Prototype]
public sealed partial class WeightedRandomOrePrototype : IWeightedRandomPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("weights", customTypeSerializer: typeof(PrototypeIdDictionarySerializer<float, OrePrototype>))]
    public Dictionary<string, float> Weights { get; private set; } = new();
}
