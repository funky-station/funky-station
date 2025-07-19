// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 beck-thompson <107373427+beck-thompson@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Chat;
using Content.Server.Speech.Components;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class VoiceOverrideSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VoiceOverrideComponent, TransformSpeakerNameEvent>(OnTransformSpeakerName);
    }

    private void OnTransformSpeakerName(Entity<VoiceOverrideComponent> entity, ref TransformSpeakerNameEvent args)
    {
        if (!entity.Comp.Enabled)
            return;

        args.VoiceName = entity.Comp.NameOverride ?? args.VoiceName;
        args.SpeechVerb = entity.Comp.SpeechVerbOverride ?? args.SpeechVerb;
    }
}
