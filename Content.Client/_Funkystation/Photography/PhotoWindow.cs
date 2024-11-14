using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client.Photography;

public sealed class PhotoWindow : BaseWindow
{
    [Dependency] private PhotoSystem _photoSystem = default!;
    private TextureRect _photo;

    public PhotoWindow()
    {
        var topContainer = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
        };

        _photo = new TextureRect();

        topContainer.AddChild(_photo);
    }

    public void Populate(string photoId)
    {
        var photo = _photoSystem.OnPhotoWindowOpen(photoId);
    }
}
