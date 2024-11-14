using Robust.Shared.GameStates;

namespace Content.Shared.Photography;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotoComponent : Component
{
    [ViewVariables] public int PhotoId = 0;
    [ViewVariables] public bool PhotoDried = false;
}

public sealed class RequestPhotoUi(string photoId) : EntityEventArgs
{
    public readonly string PhotoId = photoId;
}

public sealed class RequestPhotoResponse(string id, byte[] photo, bool loaded) : EntityEventArgs
{
    public readonly string PhotoId = id;
    public readonly byte[] PhotoData = photo;
    public readonly bool Loaded = loaded;
}

public sealed class TookPhotoResponse(EntityUid author, byte[] data, bool suicide) : EntityEventArgs
{
    public readonly byte[] PhotoData = data;
    public readonly EntityUid Author = author;
    public readonly bool Suicide = suicide;
}
