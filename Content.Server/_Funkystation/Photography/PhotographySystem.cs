using Content.Shared.Interaction;
using Content.Shared.Photography;
using Robust.Server.GameObjects;

namespace Content.Server.Photography;

public sealed class PhotographySystem : SharedPhotoSystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;

    private int _photoId = 0;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoComponent, AfterInteractEvent>(OnAfterInteract);
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
                ? new RequestPhotoResponse(photoBytes, true)
                : new RequestPhotoResponse([], false),
            eventArgs.SenderSession.Channel);
    }
}
