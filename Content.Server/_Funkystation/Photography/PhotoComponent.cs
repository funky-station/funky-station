using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server.Photography;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhotoComponent : Component
{
    /// <summary>
    /// The zoom of the viewport camera.
    /// </summary>
    [DataField]
    public Vector2 CameraZoom { get; private set; } = Vector2.One;

    [ViewVariables]
    public Vector2i Size { get; private set; } = new(200, 200);

    [ViewVariables]
    public List<NetEntity> Entities { get; set; } = new();

    [ViewVariables]
    public FormattedMessage Descriptor = new();

    [ViewVariables]
    public PhotoSession? Session;
}
