// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Prototypes;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server._Funkystation.Genetics.Mutations.Components;

[RegisterComponent]
public sealed partial class ChemicalResistanceComponent : Component
{
    /// <summary>
    /// List of reagent prototype IDs that this mutation grants resistance to.
    /// If a reagent is in this list, it will be slowly purged instead of metabolized.
    /// </summary>
    [DataField]
    public List<ProtoId<ReagentPrototype>> Reagents { get; private set; } = new();

    /// <summary>
    /// How much of the resistant reagent to remove per metabolism tick.
    /// 1 = standard purge rate.
    /// </summary>
    [DataField]
    public FixedPoint2 PurgeAmount { get; private set; } = FixedPoint2.New(1);
}
