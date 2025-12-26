using Content.Shared.StationEvents.Events;
using Robust.Client.Graphics;
using System.Linq;

namespace Content.Client.StationEvents;

public sealed partial class SolarFlareVisualsSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayManager = default!;

    private SolarFlareOverlay? _overlay = null;
    private readonly Dictionary<NetEntity, bool> _solarFlares = [];

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<SolarFlareStartedEvent>(OnStarted);
        SubscribeNetworkEvent<SolarFlareEndedEvent>(OnEnded);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (_overlay is not { } overlay)
            return;

        overlay.UpdateAlpha(frameTime);

        if (_overlay.FadeState != SolarFlareVisualsFadeState.FadeOut)
            return;

        if (overlay.IsVisible())
            return;

        if (!AnyActiveSolarFlares())
        {
            _solarFlares.Clear();
            _overlayManager.RemoveOverlay(overlay);
            _overlay = null;
        }
    }

    private void OnStarted(SolarFlareStartedEvent args)
    {
        if (!_solarFlares.Any())
        {
            _overlay = new();
            _overlayManager.AddOverlay(_overlay);
            _overlay.StartFadeIn();
        }
        // someone started fading out before us so fade back in
        else if (AnyActiveSolarFlares())
        {
            _overlay!.StartFadeIn();
        }

        _solarFlares[args.Gamerule] = false;

    }

    private void OnEnded(SolarFlareEndedEvent args)
    {
        _solarFlares[args.Gamerule] = true;

        // we are the last flare
        if (!AnyActiveSolarFlares())
            _overlay?.StartFadeOut();
    }

    private bool AnyActiveSolarFlares()
    {
        foreach (var (_, fadingOut) in _solarFlares)
        {
            if (!fadingOut)
                return true;
        }

        return false;
    }
}
