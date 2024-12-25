using System.Numerics;
using Content.Server.Tabletop;
using Content.Shared.Photography;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Photography;

// basically takes the Tabletop System and reworks it for Photography.
public sealed partial class PhotographySystem
{
    [Dependency] private readonly EyeSystem _eye = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriberSystem = default!;

    public PhotoSession EnsureSession(PhotoComponent photoComponent, HashSet<EntityUid>? scene)
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

    public void SetupScene(HashSet<EntityUid> scene)
    {
        foreach (var entId in scene)
        {
            var meta = MetaData(entId);
            var ent = Spawn(meta.EntityPrototype!.ToString());
        }
    }

    /// <summary>
    ///     Adds a player to a tabletop game session, sending a message so the tabletop window opens on their end.
    /// </summary>
    /// <param name="player">The player session in question.</param>
    /// <param name="uid">The UID of the photo entity.</param>
    public void OpenSessionFor(ICommonSession player, EntityUid uid)
    {
        if (!TryComp<PhotoComponent>(uid, out var photoComponent) || player.AttachedEntity is not {Valid: true} attachedEntity)
            return;

        // Make sure we have a session, and add the player to it if not added already.
        var session = EnsureSession(photoComponent, null);

        if (session.Players.ContainsKey(player))
            return;

        var camera = CreateCamera(photoComponent, player);

        session.Players[player] = new PhotoSessionPlayerData(camera);

        // Tell the gamer to open a viewport for the tabletop game
        RaiseNetworkEvent(new PhotoViewEvent(GetNetEntity(uid), GetNetEntity(camera), "title", photoComponent.Size), player.Channel);
    }

    private void CleanupSession(EntityUid uid)
    {
        if (!TryComp<PhotoComponent>(uid, out var photoComponent))
            return;

        if (photoComponent.Session is not { } session)
            return;

        foreach (var (player, _) in session.Players)
        {
            CloseSessionFor(player, uid);
        }

        foreach (var euid in session.Entities)
        {
            EntityManager.QueueDeleteEntity(euid);
        }

        photoComponent.Session = null;
    }

    private void CloseSessionFor(ICommonSession player, EntityUid uid)
    {
        if (!TryComp<PhotoComponent>(uid, out var photoComponent))
            return;

        if (photoComponent.Session!.Players.TryGetValue(player, out var data))
            return;

        if (data == null)
            return;

        photoComponent.Session.Players.Remove(player);
        photoComponent.Session.Entities.Remove(data.Camera);

        EntityManager.QueueDeleteEntity(data.Camera);
    }

    /// <summary>
    ///     A helper method that creates a camera for a specified player, in a tabletop game session.
    ///     Taken from
    /// </summary>
    /// <param name="photo">The tabletop game component in question.</param>
    /// <param name="player">The player in question.</param>
    /// <param name="offset">An offset from the tabletop position for the camera. Zero by default.</param>
    /// <returns>The UID of the camera entity.</returns>
    private EntityUid CreateCamera(PhotoComponent photo, ICommonSession player, Vector2 offset = default)
    {
        DebugTools.AssertNotNull(photo.Session);

        var session = photo.Session!;

        // Spawn an empty entity at the coordinates
        var camera = EntityManager.SpawnEntity(null, session.Position.Offset(offset));

        // Add an eye component and disable FOV
        var eyeComponent = EnsureComp<EyeComponent>(camera);
        _eye.SetDrawFov(camera, false, eyeComponent);
        _eye.SetZoom(camera, photo.CameraZoom, eyeComponent);

        // Add the user to the view subscribers. If there is no player session, just skip this step
        _viewSubscriberSystem.AddViewSubscriber(camera, player);

        return camera;
    }
}

public sealed class PhotoSessionPlayerData(EntityUid camera)
{
    public EntityUid Camera { get; set; }
};
