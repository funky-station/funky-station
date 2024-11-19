using Robust.Shared.Serialization;

namespace Content.Shared.Photography;

[Serializable, NetSerializable]
public sealed class PhotoViewEvent(NetEntity tableUid, NetEntity cameraUid, string title, Vector2i size)
    : EntityEventArgs
{
    public NetEntity TableUid = tableUid;
    public NetEntity CameraUid = cameraUid;
    public string Title = title;
    public Vector2i Size = size;
}
