using Content.Client.Overlays;
using Content.Shared.GameTicking;
using Content.Shared.NightVision.Components;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client.NightVision;

public sealed class NightVisionSystem : EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;


    private NightVisionOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, AfterAutoHandleStateEvent>(SyncClientComponent);

        _overlay = new(Color.Green);
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> component)
    {
        base.UpdateInternal(component);

        foreach (var comp in component.Components)
        {
            _overlay.NightvisionColor = comp.NightVisionColor;
            if (comp.IsNightVision)
                _lightManager.DrawLighting = false;
        }
        // remove the old overlay if it exists, always override with a new overlay if we have one.
        if (_overlayMan.HasOverlay<NightVisionOverlay>())
        {
            _overlayMan.RemoveOverlay<NightVisionOverlay>();
        }
        _overlayMan.AddOverlay(_overlay);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();
        _overlayMan.RemoveOverlay(_overlay);
        _lightManager.DrawLighting = true;
    }

    private void SyncClientComponent(EntityUid uid, NightVisionComponent component, ref AfterAutoHandleStateEvent handleEvent)
    {
        // not implemented, 
        // Somehow the client color is not matching the Server color. IDK if its an issue with the component copy
        // constructor or what, but someone can implement this if they want custumizable Nv colors, or track down
        // whatever other issue is making the color in the client component incorrect/ default. (it is correct in the server component)
    }
}
