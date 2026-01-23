// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Managers;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server.Preferences;

public sealed class AltTitleReminderSystem : EntitySystem
{
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ILocalizationManager _loc = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawn);
    }

    private void OnPlayerSpawn(PlayerSpawnCompleteEvent ev)
    {
        if (ev.Profile is not HumanoidCharacterProfile profile)
            return;

        if (string.IsNullOrEmpty(ev.JobId) || !_prototype.TryIndex<JobPrototype>(ev.JobId, out var jobProto))
            return;

        if (profile.JobAlternateTitles.TryGetValue(jobProto.ID, out var altTitleId) &&
            _prototype.TryIndex<JobAlternateTitlePrototype>(altTitleId, out var altTitleProto))
        {
            var message = _loc.GetString("job-alt-title-reminder",
                ("altTitle", altTitleProto.LocalizedName),
                ("jobName", jobProto.LocalizedName));

            var wrappedMessage = _loc.GetString("chat-manager-server-wrap-message", ("message", message));

            _chat.ChatMessageToOne(
                ChatChannel.Server,
                message,
                wrappedMessage,
                default,
                false,
                ev.Player.Channel,
                colorOverride: null);
        }
    }
}
