using Robust.Shared.GameStates;

namespace Content.Shared._Funkystation.Atmos.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BluespaceGasUtilizerComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? BluespaceSender;
}
