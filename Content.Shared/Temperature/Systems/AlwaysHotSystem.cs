// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;

namespace Content.Shared.Temperature.Systems;

public sealed class AlwaysHotSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlwaysHotComponent, IsHotEvent>(OnIsHot);
    }

    private void OnIsHot(Entity<AlwaysHotComponent> ent, ref IsHotEvent args)
    {
        args.IsHot = true;
    }
}
