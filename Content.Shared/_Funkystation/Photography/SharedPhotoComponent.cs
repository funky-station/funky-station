namespace Content.Shared.Photography;

public sealed partial class SharedPhotoComponent : Component
{
    public string Name => "Photo";
}

public sealed class RequestPhotoUiMessage(string photoId) : EntityEventArgs
{
    public readonly string PhotoId = photoId;
}

public sealed class RequestPhotoResponse(byte[] photo, bool loaded) : EntityEventArgs
{
    public readonly byte[] PhotoData = photo;
    public readonly bool Loaded = loaded;
}
