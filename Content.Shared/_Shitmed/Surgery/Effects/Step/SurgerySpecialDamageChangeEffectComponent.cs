// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
namespace Content.Shared._Shitmed.Medical.Surgery.Effects.Step;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgerySpecialDamageChangeEffectComponent : Component
{
    [DataField]
    public string DamageType = "";

    [DataField]
    public bool IsConsumable;
}