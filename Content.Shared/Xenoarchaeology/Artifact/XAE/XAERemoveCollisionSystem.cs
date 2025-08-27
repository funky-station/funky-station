// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Xenoarchaeology.Artifact.XAE.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Xenoarchaeology.Artifact.XAE;

/// <summary>
/// System for xeno artifact effect that make artifact pass through other objects.
/// </summary>
public sealed class XAERemoveCollisionSystem : BaseXAESystem<XAERemoveCollisionComponent>
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    /// <inheritdoc />
    protected override void OnActivated(Entity<XAERemoveCollisionComponent> ent, ref XenoArtifactNodeActivatedEvent args)
    {
        if (!TryComp<FixturesComponent>(ent.Owner, out var fixtures))
            return;

        foreach (var fixture in fixtures.Fixtures.Values)
        {
            _physics.SetHard(ent.Owner, fixture, false, fixtures);
        }
    }
}
