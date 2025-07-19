// SPDX-FileCopyrightText: 2022 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Dictionary;
using Content.Shared.FixedPoint;
using Content.Shared._White.StoreDiscount;

namespace Content.Shared.Store;

/// <summary>
///     Specifies generic info for initializing a store.
/// </summary>
[Prototype]
[DataDefinition]
public sealed partial class StorePresetPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The name displayed at the top of the store window
    /// </summary>
    [DataField("storeName", required: true)]
    public string StoreName { get; private set; } = string.Empty;

    /// <summary>
    /// The categories that this store can access
    /// </summary>
    [DataField("categories", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<StoreCategoryPrototype>))]
    public HashSet<string> Categories { get; private set; } = new();

    /// <summary>
    /// The inital balance that the store initializes with.
    /// </summary>
    [DataField("initialBalance",
        customTypeSerializer: typeof(PrototypeIdDictionarySerializer<FixedPoint2, CurrencyPrototype>))]
    public Dictionary<string, FixedPoint2>? InitialBalance { get; private set; }

    /// <summary>
    /// The currencies that are accepted in the store
    /// </summary>
    [DataField("currencyWhitelist", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<CurrencyPrototype>))]
    public HashSet<string> CurrencyWhitelist { get; private set; } = new();
}
