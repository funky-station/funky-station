using Robust.Shared.Map.Components;

namespace Content.Server.Photography;

// basically takes the Tabletop System and reworks it for Photography.
public sealed partial class PhotographySystem
{
    public PhotoSession EnsureSession(PhotoComponent photoComponent, HashSet<EntityUid> scene)
    {
        if (photoComponent.Session != null)
            return photoComponent.Session;

        EnsureSceneMap();

        if (!TryComp<MapComponent>(SceneMap, out var mapComponent))
        {
            throw new InvalidOperationException($"Component {nameof(MapComponent)} is not attached to scene map.");
        }

        photoComponent.Session = new PhotoSession(mapComponent.MapId, GetNextTabletopPosition());

        return photoComponent.Session;
    }
}
