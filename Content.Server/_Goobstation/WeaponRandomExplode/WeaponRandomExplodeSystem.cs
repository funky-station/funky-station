// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Random;
using Content.Shared.Weapons.Ranged.Events;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Power.EntitySystems;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;

namespace Content.Server._Goobstation.WeaponRandomExplode;

public sealed class WeaponRandomExplodeSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<WeaponRandomExplodeComponent, ShotAttemptedEvent>(OnShot);
    }

    private void OnShot(EntityUid uid, WeaponRandomExplodeComponent component, ShotAttemptedEvent args)
    {
        if (component.explosionChance <= 0)
            return;

        if (!TryComp<BatteryComponent>(uid, out var battery))
            return;

        var charge = _battery.GetCharge(uid);

        if (charge <= 0f)
            return;

        if (!_random.Prob(component.explosionChance))
            return;

        var reduction = component.reduction is not null
            ? Convert.ToInt32(component.reduction)
            : 1;

        var intensity = 1;

        if (component.multiplyByCharge > 0)
        {
            intensity = Convert.ToInt32(
                component.multiplyByCharge * (charge / (100f * reduction))
            );
        }

        _explosionSystem.QueueExplosion(
            uid,
            typeId: "Default",
            totalIntensity: intensity,
            slope: 5f / reduction,
            maxTileIntensity: 10f / reduction
        );

        if (component.destroyGun)
            QueueDel(uid);
    }
}
