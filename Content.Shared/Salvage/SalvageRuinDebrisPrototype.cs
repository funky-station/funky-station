// SPDX-FileCopyrightText: 2025 Terkala <appleorange64@gmail.com>
// SPDX-FileCopyrightText: 2025 terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Salvage;

/// <summary>
/// Prototype for configuring debris entities that spawn on salvage ruins.
/// </summary>
[Prototype("salvageRuinDebris")]
public sealed partial class SalvageRuinDebrisPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// List of debris entities to spawn with their relative spawn chances.
    /// </summary>
    [DataField("entries", required: true)]
    public List<SalvageRuinDebrisEntry> Entries = new();
}

/// <summary>
/// An entry for a debris entity to spawn in salvage ruins.
/// </summary>
[DataDefinition]
public sealed partial class SalvageRuinDebrisEntry
{
    /// <summary>
    /// The prototype ID of the entity to spawn.
    /// </summary>
    [DataField("proto", required: true, customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string Proto = string.Empty;

    /// <summary>
    /// The relative spawn chance for this entity (normalized with other entries).
    /// Higher values mean more likely to spawn.
    /// </summary>
    [DataField("chance")]
    public float Chance = 1.0f;
}

