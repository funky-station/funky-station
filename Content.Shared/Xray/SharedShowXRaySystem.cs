using Robust.Shared.GameStates;

namespace Content.Shared.XRay;

public sealed class SharedShowXRaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowXRayComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, ShowXRayComponent component, ref ComponentGetState args)
    {
        args.State = new ShowXRayComponentState(
            component.Enabled,
            component.Shader,
            component.EntityRange,
            component.TileRange
        );
    }
}
