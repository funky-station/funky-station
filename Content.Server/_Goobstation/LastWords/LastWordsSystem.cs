// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2026 phmnsx <lynnwastinghertime@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chat.Systems;
using Content.Shared.Mind.Components;
using Content.Shared.Mind;
using Content.Shared._Goobstation.LastWords;
using Content.Shared.Mobs.Components;

namespace Content.Server._Goobstation.LastWords;

public sealed class LastWordsSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MobStateComponent, EntitySpokeEvent>(OnEntitySpoke);
    }

    private void OnEntitySpoke(EntityUid uid, MobStateComponent _, EntitySpokeEvent args)
    {
        _mindSystem.TryGetMind(uid, out var mindId, out var _);

        if (TryComp<LastWordsComponent>(mindId, out var lastWordsComp))
            lastWordsComp.LastWords = args.Message;
    }
}
