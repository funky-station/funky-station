using Content.Shared.DeviceLinking;
using Robust.Shared.Prototypes;

namespace Content.Server.Medical;

[RegisterComponent]
public sealed partial class BodyScannerComponent : Component
{
    [DataField]
    public ProtoId<SinkPortPrototype> OperatingTablePort = "OperatingTableReceiver";

    [DataField, AutoNetworkedField]
    public EntityUid? OperatingTable;
}
