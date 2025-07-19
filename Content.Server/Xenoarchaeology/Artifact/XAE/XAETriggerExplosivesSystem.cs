// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Explosion.EntitySystems;
using Content.Server.Xenoarchaeology.Artifact.XAE.Components;
using Content.Shared.Explosion.Components;
using Content.Shared.Xenoarchaeology.Artifact;
using Content.Shared.Xenoarchaeology.Artifact.XAE;

namespace Content.Server.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// System for xeno artifact effect of triggering explosion.
/// </summary>
public sealed class XAETriggerExplosivesSystem : BaseXAESystem<XAETriggerExplosivesComponent>
{
    [Dependency] private readonly ExplosionSystem _explosion = default!;

    /// <inheritdoc />
    protected override void OnActivated(Entity<XAETriggerExplosivesComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        if(!TryComp<ExplosiveComponent>(ent, out var explosiveComp))
            return;

        _explosion.TriggerExplosive(ent, explosiveComp);
    }
}
