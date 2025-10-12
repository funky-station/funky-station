// SPDX-FileCopyrightText: 2025 Tyranex <bobthezombie4@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Radio.Components;
using Content.Shared.MalfAI;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization;

namespace Content.Server.MalfAI;

/// <summary>
/// System that handles granting syndicate radio communications to malfunction AI
/// </summary>
public sealed class MalfAiSyndicateCommsSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MalfAiMarkerComponent, MalfAiSyndicateKeysUnlockedEvent>(OnSyndicateKeysUnlocked);
    }

    private void OnSyndicateKeysUnlocked(EntityUid uid, MalfAiMarkerComponent component, MalfAiSyndicateKeysUnlockedEvent args)
    {
        // Add or get the IntrinsicRadioTransmitterComponent for sending syndicate messages
        var transmitterComp = EnsureComp<IntrinsicRadioTransmitterComponent>(uid);
        transmitterComp.Channels.Add("Syndicate");

        // Add or get the ActiveRadioComponent for receiving syndicate messages
        var activeRadioComp = EnsureComp<ActiveRadioComponent>(uid);
        activeRadioComp.Channels.Add("Syndicate");

        // IntrinsicRadioTransmitterComponent and ActiveRadioComponent are server-only and don't need network synchronization
    }
}
