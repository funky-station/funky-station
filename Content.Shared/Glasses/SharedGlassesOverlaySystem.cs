using Robust.Shared.GameStates;

namespace Content.Shared.Glasses;

public sealed class SharedGlassesOverlaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlassesOverlayComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, GlassesOverlayComponent component, ref ComponentGetState args)
    {
        args.State = new GlassesOverlayComponentState(
            component.Enabled,
            component.Shader,
            component.Color
        );
    }
}
