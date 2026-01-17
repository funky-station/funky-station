// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.CosmicCult.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicLambdaParticleComponent : Component;


[RegisterComponent, NetworkedComponent]
public sealed partial class CosmicLambdaParticleSourceComponent : Component
{
    public DoAfterId? DoAfterId = null;
}
