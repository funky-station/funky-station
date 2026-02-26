// SPDX-FileCopyrightText: 2026 phmnsx <lynnwastinghertime@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Chat.Systems;
using Content.Server.Mind;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Speech;
using Robust.Shared.Network;

namespace Content.Server._Funkystation.Manifest;

public sealed class ManifestInfoSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<MindContainerComponent, EntitySpokeEvent>(OnEntitySpeak);
        SubscribeLocalEvent<MindComponent, TransferMindEvent>(OnMindTransfered);
    }

    private void OnEntitySpeak(EntityUid uid, MindContainerComponent _, EntitySpokeEvent args)
    {
        if (_mindSystem.TryGetMind(uid, out var _, out var mind))
        {
            mind.LastMessage = args.Message;
        }
    }

    private void OnMindTransfered(EntityUid uid, MindComponent mind, TransferMindEvent args)
    {
        if (args.Target != null && !HasComp<GhostComponent>(args.Target))
        {
            mind.LastEntity = args.Target;
        }
    }
}
