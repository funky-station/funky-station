using System.IO;
using System.Threading.Tasks;
using Robust.Shared.ContentPack;
using Robust.Shared.Utility;

namespace Content.Shared.Photography;

public abstract class SharedPhotoSystem : EntitySystem
{
    public static ResPath PhotosPath = new("/Photos");

    private Dictionary<string, ResPath> _photos = new();

    [Dependency] private readonly IResourceManager _resourceManager = default!;

    protected void EnsurePhotoDirExists()
    {
        if (_resourceManager.UserData.IsDir(PhotosPath))
            return;

        _resourceManager.UserData.CreateDir(PhotosPath);
    }

    protected async Task StorePhotoImpl(byte[] data, string photoId)
    {
        Logger.InfoS("photo", "Storing a photo...");

        EnsurePhotoDirExists();

        var path = PhotosPath / $"{photoId}.png";

        await using var file = _resourceManager.UserData.Open(path, FileMode.Create);

        await using (file)
        {
            await using var writer = new BinaryWriter(file);
            foreach (var dat in data)
            {
                writer.Write(dat);
            }
        }

        Logger.InfoS("photo", $"Stored a photo to {path}");

        _photos.Add(photoId, path);
    }

    public bool TryGetPhotoPath(string photoId, out ResPath photoPath)
    {
        return _photos.TryGetValue(photoId, out photoPath);
    }

    public bool TryGetPhotoBytes(string photoId, out byte[] photoBytes)
    {
        if(TryGetPhotoPath(photoId, out var photoPath))
        {
            var photo = _resourceManager.UserData.Open(photoPath, FileMode.Open);
            photoBytes = photo.CopyToArray();
            return true;
        }

        photoBytes = [];
        return false;
    }
}
