using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.TeleportTrigger;

[RegisterComponent]
public sealed partial class TeleportOnTriggerComponent : Component
{
    [DataField]
    public EntProtoId MarkerPrototype = "LifelineMarker";

    [DataField("allowNukeDisk")]
    public bool AllowNukeDisk = false;
}
