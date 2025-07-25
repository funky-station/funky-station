// SPDX-FileCopyrightText: 2023 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents.Events;

public sealed class RandomSpawnRule : StationEventSystem<RandomSpawnRuleComponent>
{
    protected override void Started(EntityUid uid, RandomSpawnRuleComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        if (TryFindRandomTile(out _, out _, out _, out var coords))
        {
            Sawmill.Info($"Spawning {comp.Prototype} at {coords}");
            Spawn(comp.Prototype, coords);
        }
    }
}
