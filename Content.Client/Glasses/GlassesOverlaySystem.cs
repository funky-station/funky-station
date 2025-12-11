using Content.Shared.Glasses;
using Content.Shared.Inventory.Events;
using Content.Client.Overlays;
using Robust.Client.Graphics;
using System.Linq;
using Robust.Shared.GameStates;

namespace Content.Client.Glasses;

public sealed class GlassesOverlaySystem : EquipmentHudSystem<GlassesOverlayComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private GlassesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new();
        SubscribeLocalEvent<GlassesOverlayComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, GlassesOverlayComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not GlassesOverlayComponentState state)
            return;

        component.Enabled = state.Enabled;
        component.Shader = state.Shader;
        component.Color = state.Color;

        // DIRECT UPDATE (Bypassing Relay)
        // Manually refresh the local provider list
        if (component.Enabled)
        {
            _overlay.Providers.Add(component);
        }
        else
        {
            _overlay.Providers.Remove(component);
        }

        // No explicit Refresh() method needed for GlassesOverlay usually,
        // as it checks Providers.Any() in BeforeDraw, but if it has one, call it.
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<GlassesOverlayComponent> args)
    {
        base.UpdateInternal(args);

        if (!_overlayMan.HasOverlay<GlassesOverlay>())
            _overlayMan.AddOverlay(_overlay);

        // Note: The original code used a HashSet of components
        _overlay.Providers = args.Components.Where(c => c.Enabled).ToHashSet();
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();
        _overlayMan.RemoveOverlay(_overlay);
    }
}
