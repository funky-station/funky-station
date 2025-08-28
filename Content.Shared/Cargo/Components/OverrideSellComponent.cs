// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared.Cargo.Components;

/// <summary>
/// Makes a sellable object portion out its value to a specified department rather than the station default
/// </summary>
[RegisterComponent]
public sealed partial class OverrideSellComponent : Component
{
    /// <summary>
    /// The account that will receive the primary funds from this being sold.
    /// </summary>
    [DataField(required: true)]
    public ProtoId<CargoAccountPrototype> OverrideAccount;
}
