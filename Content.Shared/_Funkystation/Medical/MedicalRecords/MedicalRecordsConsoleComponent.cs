using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.StationRecords;

namespace Content.Shared._Funkystation.Medical.MedicalRecords;

/// <summary>
/// A component for Medical Records Console storing an active station record key and a currently applied filter
/// </summary>
[RegisterComponent]
[Access(typeof(SharedMedicalRecordsConsoleSystem))]
public sealed partial class MedicalRecordsConsoleComponent : Component
{
    [DataField]
    public uint? SelectedIndex { get; set; }

    [DataField]
    public StationRecordsFilter? Filter;
}
