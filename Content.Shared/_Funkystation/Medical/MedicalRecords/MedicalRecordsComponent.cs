using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Medical.MedicalRecords;

public sealed partial class MedicalRecordsComponent : Component
{
    // define system-wide vars and information here

    [Serializable, NetSerializable]
    public enum MedicalRecordsUiKey
    {
        Key
    }
}
