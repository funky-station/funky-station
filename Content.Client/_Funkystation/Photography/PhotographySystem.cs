using Content.Client._Funkystation.Photography.UI;
using Content.Shared.Photography;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Funkystation.Photography;

public sealed partial class PhotographySystem : EntitySystem
{
    private DefaultWindow? _window;

    public override void Initialize()
    {
        SubscribeNetworkEvent<PhotoViewEvent>(OnPhotoViewEvent);
    }

    private void OnPhotoViewEvent(PhotoViewEvent evt)
    {
        // Close the currently opened window, if it exists
        _window?.Close();

        // Get the camera entity that the server has created for us
        var camera = GetEntity(evt.CameraUid);

        if (!EntityManager.TryGetComponent<EyeComponent>(camera, out var eyeComponent))
        {
            // If there is no eye, print error and do not open any window
            Log.Error("Camera entity does not have eye component!");
            return;
        }

        // Create a window to contain the viewport
        _window = new PhotoWindow(eyeComponent.Eye, (evt.Size.X, evt.Size.Y))
        {
            MinWidth = 500,
            MinHeight = 436,
            Title = evt.Title,
        };

        _window.OnClose += OnWindowClose;
    }

    private void OnWindowClose()
    {
        _window = null;
    }
}
