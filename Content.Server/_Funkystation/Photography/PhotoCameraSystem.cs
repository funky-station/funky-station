using Content.Server.Photography;

namespace Content.Shared.Photography;

public sealed class PhotoCameraSystem : EntitySystem
{
    [Dependency] private readonly PhotographySystem _photography = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotoCameraComponent, PhotoCameraTakePictureEvent>(OnPhotoCameraTakePicture);
    }

    private void OnPhotoCameraTakePicture(EntityUid ent, PhotoCameraComponent photoCamera, PhotoCameraTakePictureEvent e)
    {
        _photography.CreatePhotoOnPlayer(e.Performer, e.Selfie);
    }
}
