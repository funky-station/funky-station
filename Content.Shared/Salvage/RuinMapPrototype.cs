// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared.Salvage;

[Prototype("ruinMap")]
public sealed partial class RuinMapPrototype : IPrototype
{
    [ViewVariables] [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// Relative directory path to the given map, i.e. `/Maps/bagel.yml`
    /// </summary>
    [DataField(required: true)] public ResPath MapPath;
}

