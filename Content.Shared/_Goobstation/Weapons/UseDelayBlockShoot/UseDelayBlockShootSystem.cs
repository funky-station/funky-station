// SPDX-FileCopyrightText: 2024 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Timing;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._Goobstation.Weapons.UseDelayBlockShoot;

public sealed class UseDelayBlockShootSystem : EntitySystem
{
    [Dependency] private readonly UseDelaySystem _useDelay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<UseDelayBlockShootComponent, AttemptShootEvent>(OnShootAttempt);
    }

    private void OnShootAttempt(Entity<UseDelayBlockShootComponent> ent, ref AttemptShootEvent args)
    {
        if (TryComp(ent, out UseDelayComponent? useDelay) && _useDelay.IsDelayed((ent, useDelay)))
            args.Cancelled = true;
    }
}
