// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._DV.CosmicCult.Components;
using Content.Shared.EntityEffects;

namespace Content.Server._DV.CosmicCult;

/// <summary>
/// Cleanses cult membership from an entity.
/// </summary>
public sealed partial class CleanseCultSystem : EntityEffectSystem<CosmicCultComponent, CleanseCult>
{
    protected override void Effect(Entity<CosmicCultComponent> entity, ref EntityEffectEvent<CleanseCult> args)
    {
        if (HasComp<CosmicCultComponent>(entity) || HasComp<RogueAscendedInfectionComponent>(entity))
            EnsureComp<CleanseCultComponent>(entity); // We just slap them with the component and let the Deconversion system handle the rest.
    }
}
