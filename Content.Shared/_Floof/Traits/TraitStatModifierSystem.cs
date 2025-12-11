// SPDX-FileCopyrightText: 2024 fox <daytimer253@gmail.com>
// SPDX-FileCopyrightText: 2025 sleepyyapril <123355664+sleepyyapril@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared._EE.Contests;
using Content.Shared._Floof.Traits.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Floof.Traits;

public sealed partial class TraitStatModifierSystem : EntitySystem
{
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly FixtureSystem _fixtures = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<FixtureDensityModifierComponent, MapInitEvent>(OnInitDensity); // Traits are added after CharacterSpawnedEvent so it's fine™
    }

    private void OnInitDensity(Entity<FixtureDensityModifierComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<FixturesComponent>(ent.Owner, out var fixtures))
            return;

        foreach (var (id, fix) in fixtures.Fixtures)
        {
            if (!fix.Hard || fix.Density < ent.Comp.Min || fix.Density > ent.Comp.Max)
                continue;

            var result = Math.Clamp(fix.Density * ent.Comp.Factor, ent.Comp.Min, ent.Comp.Max);
            _physics.SetDensity(ent, id, fix, result, update: false, fixtures);
        }

        _fixtures.FixtureUpdate(ent, true, true, fixtures);
    }
}
