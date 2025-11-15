using Robust.Shared.GameStates;
using Robust.Shared.Audio;

namespace Content.Shared.Tools.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FancyLighterComponent : Component
{
    /// <summary>
    /// The stupid music the lighter should play when switched on.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? SoundActivate;
}
