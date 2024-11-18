using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Server.Photography;

[RegisterComponent, NetworkedComponent]
public sealed partial class PhotoComponent : Component
{
    [ViewVariables]
    public List<NetEntity> Entities { get; set; } = new();

    [ViewVariables]
    public FormattedMessage Descriptor = new();

    [ViewVariables]
    public PhotoSession? Session;
}
