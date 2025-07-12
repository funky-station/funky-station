using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.StationRecords;

namespace Content.Server._Funkystation.Medical.MedicalRecords;

[RegisterComponent]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly)]
    public uint? SelectedIndex { get; set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public StationRecordsFilter? Filter;
}
