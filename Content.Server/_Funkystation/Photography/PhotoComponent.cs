using Robust.Shared.GameStates;

namespace Content.Server.Photography;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PhotoComponent : Component
{
    [ViewVariables] public int PhotoId = 0;
    [ViewVariables] public bool PhotoDried = false;
}
