using Robust.Shared.GameObjects;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Medical;

[RegisterComponent, NetworkedComponent]
public sealed partial class BedWheelsComponent : Component
{
    [DataField]
    public bool Locked = true;
}
