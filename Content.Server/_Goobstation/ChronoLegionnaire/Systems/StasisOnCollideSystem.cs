// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using JetBrains.Annotations;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Events;

namespace Content.Server._Goobstation.ChronoLegionnaire;

[UsedImplicitly]
public sealed class StasisOnCollideSystem : EntitySystem
{
    [Dependency] private readonly StasisSystem _stasisSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StasisOnCollideComponent, StartCollideEvent>(HandleCollide);
        SubscribeLocalEvent<StasisOnCollideComponent, ThrowDoHitEvent>(HandleThrow);
    }

    private void TryCollideStasis(Entity<StasisOnCollideComponent> projectile, EntityUid target)
    {
        if (EntityManager.TryGetComponent<StatusEffectsComponent>(target, out var status))
        {
            _stasisSystem.TryStasis((target, status), true, projectile.Comp.StasisTime);
        }
    }

    /// <summary>
    /// Check if projectile hits another entity
    /// </summary>
    private void HandleCollide(Entity<StasisOnCollideComponent> projectile, ref StartCollideEvent args)
    {
        if (args.OurFixtureId != projectile.Comp.FixtureID)
            return;

        TryCollideStasis(projectile, args.OtherEntity);
    }

    /// <summary>
    /// For throwing (in chrono bola case)
    /// </summary>
    private void HandleThrow(Entity<StasisOnCollideComponent> projectile, ref ThrowDoHitEvent args)
    {
        TryCollideStasis(projectile, args.Target);
    }

}
