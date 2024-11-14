namespace Content.Shared.Photography;

public sealed partial class SharedPhotoComponent : Component
{
    public string Name => "Photo";
}

public sealed class RequestPhotoUi(string photoId) : EntityEventArgs
{
    public readonly string PhotoId = photoId;
}

public sealed class RequestPhotoResponse(byte[] photo, bool loaded) : EntityEventArgs
{
    public readonly byte[] PhotoData = photo;
    public readonly bool Loaded = loaded;
}

public sealed class TookPhotoResponse(EntityUid author, byte[] data, bool suicide) : EntityEventArgs
{
    public readonly byte[] PhotoData = data;
    public readonly EntityUid Author = author;
    public readonly bool Suicide = suicide;
}
