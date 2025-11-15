// SPDX-FileCopyrightText: 2025 BuildTools <unconfigured@null.spigotmc.org>
// SPDX-FileCopyrightText: 2025 mycobiota <154991750+mycobiota@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tools.Components;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;


namespace Content.Shared.Tools.Systems;

public sealed class FancyLighterSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    private EntityUid? _audioUid;
    private AudioComponent? _audioComponent;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FancyLighterComponent, ItemToggleDeactivateAttemptEvent>(OnLighterClose);
        SubscribeLocalEvent<FancyLighterComponent, ItemToggleActivateAttemptEvent>(OnLighterOpen);
    }

    private void OnLighterClose(EntityUid uid, FancyLighterComponent component, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (!args.Cancelled)
        {
            _audio.Stop(_audioUid, _audioComponent);
        }
    }

    private void OnLighterOpen(EntityUid uid, FancyLighterComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (!args.Cancelled)
        {
            (EntityUid, AudioComponent) audio = _audio.PlayPvs(component.SoundActivate, uid).GetValueOrDefault();
            _audioUid = audio.Item1;
            _audioComponent = audio.Item2;
        }
    }
}
