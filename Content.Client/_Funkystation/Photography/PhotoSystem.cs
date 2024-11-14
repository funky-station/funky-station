using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Content.Shared.Photography;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Shared.ContentPack;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Content.Client.Photography;

public sealed class PhotoSystem : SharedPhotoSystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IResourceManager _resourceManager = default!;
    [Dependency] private readonly ISawmill _logger = default!;
    [Dependency] private readonly IClyde _clyde = default!;

    public override void Initialize()
    {
        base.Initialize();

        IoCManager.InjectDependencies(this);

        SubscribeNetworkEvent<RequestPhotoResponse>(OnRequestPhotoResponse);
    }

    public async void StorePhoto(byte[] data, string photoId)
    {
        await StorePhotoImpl(data, photoId);
    }

    public async Task<ResPath> StoreImagePNG(Image<Rgb24> image)
    {
        var photoId = "LAST_PHOTO_TAKEN.png";
        var success = false;
        ResPath path;

        EnsurePhotoDirExists();

        for (var i = 0; i < 5; i++)
        {
            try
            {
                if (i != 0)
                {
                    photoId = $"LAST_PHOTO_TAKEN-{i}.png";
                }

                path = PhotosPath / photoId;

                await using var file =
                    _resourceManager.UserData.Open(path, FileMode.OpenOrCreate);

                await Task.Run(() =>
                {
                    image.SaveAsPng(file);
                });

                return path;
            }
            catch (IOException e)
            {
                _logger.Error("Failed to save photo, retrying?:\n{0}", e);
            }
        }

        if (!success)
        {
            _logger.Error("Unable to save photo.");
        }

        return new ResPath(null!);
    }

    private void OnRequestPhotoResponse(RequestPhotoResponse msg, EntitySessionEventArgs args)
    {
        if (TryGetPhotoBytes("test", out _) | !msg.Loaded)
            return;

        StorePhoto(msg.PhotoData, msg.PhotoId);

        // todo: open window when photo is loaded
        // gotta do it this way cuz i cant figure it out another way
    }

    public byte[]? OnPhotoWindowOpen(string photoId)
    {
        if (TryGetPhotoBytes(photoId, out var bytes))
            return bytes;

        RaiseNetworkEvent(new RequestPhotoUi(photoId));

        return null;
    }

    public async void TryTakePhoto(EntityUid author, Vector2 photoCenter, float radius)
    {
        var screenshot = await _clyde.ScreenshotAsync(ScreenshotType.Final);
        var cropDimensions = EyeManager.PixelsPerMeter * (radius * 4);

        var cropX = (int) Math.Clamp(Math.Floor(photoCenter.X - cropDimensions / 2), 0, screenshot.Width - cropDimensions);
        var cropY = (int) Math.Clamp(Math.Floor(photoCenter.Y - cropDimensions / 2), 0, screenshot.Height - cropDimensions);

        using (screenshot)
        {
            //Crop screenshot to photo dimensions
            screenshot.Mutate(
                i => i.Crop(new Rectangle(cropX, cropY, (int) cropDimensions, (int) cropDimensions))
            );

            //Store it to disk as a PNG
            var path = await StoreImagePNG(screenshot);

            await using var file =
                _resourceManager.UserData.Open(path, FileMode.Open);

            RaiseNetworkEvent(new TookPhotoResponse(file.CopyToArray(), false));
        }
    }
}
