// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 deltanedas <39013340+deltanedas@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;

namespace Content.Server.StationEvents.Events;

public sealed class KudzuGrowthRule : StationEventSystem<KudzuGrowthRuleComponent>
{
    protected override void Started(EntityUid uid, KudzuGrowthRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Pick a place to plant the kudzu.
        if (!TryFindRandomTile(out var targetTile, out _, out var targetGrid, out var targetCoords))
            return;
        Spawn("Kudzu", targetCoords);
        Sawmill.Info($"Spawning a Kudzu at {targetTile} on {targetGrid}");

    }
}
