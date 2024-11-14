using Content.Shared.Interaction;
using Content.Shared.Photography;
using Robust.Server.GameObjects;

namespace Content.Server.Photography;

public sealed class PhotographySystem : SharedPhotoSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    private int _photoId = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<TookPhotoResponse>(OnTookPhotoResponse);

        SubscribeLocalEvent<PhotoComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnTookPhotoResponse(TookPhotoResponse tookPhoto, EntitySessionEventArgs args)
    {
        // todo: i understand this needs to be checked for ridiculous sizes and other factors that could break the server. no, i dont care rn
        var authorEnt = args.SenderSession.AttachedEntity;

        if (authorEnt == null)
            return;

        if (!TryComp<TransformComponent>(authorEnt, out var transform))
            return;

        var photo = Spawn("Photo", transform.Coordinates);

        if (!TryComp<PhotoComponent>(photo, out var photoComponent))
            return;

        if (tookPhoto.Suicide)
        {
            //todo: do suicide functionality
        }

        StorePhoto(tookPhoto.PhotoData, photoComponent);
    }

    private void OnAfterInteract(EntityUid uid, PhotoComponent component, AfterInteractEvent args)
    {

    }

    public async void StorePhoto(byte[] data, PhotoComponent photo)
    {
        photo.PhotoId = _photoId++;
        await StorePhotoImpl(data, photo.PhotoId.ToString());
    }

    private void RequestPhoto(RequestPhotoUi request, EntitySessionEventArgs eventArgs)
    {
        RaiseNetworkEvent(
            TryGetPhotoBytes(request.PhotoId, out var photoBytes)
                ? new RequestPhotoResponse(request.PhotoId, photoBytes, true)
                : new RequestPhotoResponse(request.PhotoId, [], false),
            eventArgs.SenderSession.Channel);
    }
}
