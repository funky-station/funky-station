// SPDX-FileCopyrightText: 2025 BuildTools <unconfigured@null.spigotmc.org>
// SPDX-FileCopyrightText: 2025 mycobiota <154991750+mycobiota@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tools.Components;
using Content.Shared.Item.ItemToggle.Components;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;


namespace Content.Shared.Tools.Systems;

public sealed class CheapLighterSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private EntityUid? _audioUid;
    private AudioComponent? _audioComponent;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CheapLighterComponent, ItemToggleDeactivateAttemptEvent>(OnLighterClose);
        SubscribeLocalEvent<CheapLighterComponent, ItemToggleActivateAttemptEvent>(OnLighterOpen);
    }

    private void OnLighterClose(EntityUid uid, CheapLighterComponent component, ref ItemToggleDeactivateAttemptEvent args)
    {
        if (!args.Cancelled)
        {
            _audio.Stop(_audioUid, _audioComponent);
        }
    }

    private void OnLighterOpen(EntityUid uid, CheapLighterComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        if (_random.NextFloat() < component.FailChance)
        {
            args.Cancelled = true;
            _audio.PlayPvs(component.SoundFail, uid);
            return;
        }

        if (!args.Cancelled && component.SoundActivate != null)
        {
            (EntityUid, AudioComponent) audio = _audio.PlayPvs(component.SoundActivate, uid).GetValueOrDefault();
            _audioUid = audio.Item1;
            _audioComponent = audio.Item2;
        }
    }
}
