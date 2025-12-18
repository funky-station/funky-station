using Robust.Shared.GameStates;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Medical;

[RegisterComponent, NetworkedComponent]
public sealed partial class BedTankVisualComponent : Component
{
    [DataField(required: true)]
    public BedTankVisual Visual;
}
