// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component to call back to the cosmic cult ability system regarding a collision
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CosmicAstralNovaComponent : Component
{
    [DataField]
    public bool DoStun = true;

    [DataField]
    public DamageSpecifier CosmicNovaDamage = new()
    {
        DamageDict = new() {
            { "Asphyxiation", 13 }
        }
    };
}
