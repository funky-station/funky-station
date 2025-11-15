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
        _audio.Stop(_audioUid, _audioComponent);
    }

    private void OnLighterOpen(EntityUid uid, FancyLighterComponent component, ref ItemToggleActivateAttemptEvent args)
    {
        (EntityUid, AudioComponent) audio = _audio.PlayPvs(component.SoundActivate, uid).GetValueOrDefault();
        _audioUid = audio.Item1;
        _audioComponent = audio.Item2;
    }
}
