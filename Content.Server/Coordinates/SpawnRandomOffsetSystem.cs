// SPDX-FileCopyrightText: 2022 Vera Aguilera Puerto <6766154+Zumorica@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Random;
using Content.Shared.Random.Helpers;
using Robust.Shared.Random;

namespace Content.Server.Coordinates;

public sealed class SpawnRandomOffsetSystem : EntitySystem
{
    [Dependency] private readonly RandomHelperSystem _randomHelper = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnRandomOffsetComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, SpawnRandomOffsetComponent component, MapInitEvent args)
    {
        _randomHelper.RandomOffset(uid, component.Offset);
        EntityManager.RemoveComponentDeferred(uid, component);
    }
}
