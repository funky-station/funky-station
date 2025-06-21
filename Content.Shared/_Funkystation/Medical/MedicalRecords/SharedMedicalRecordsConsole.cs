using Content.Shared._Funkystation.Records;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Medical.MedicalRecords;

[Serializable, NetSerializable]
public enum MedicalRecordsConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleState : BoundUserInterfaceState
{
    [Serializable, NetSerializable]
    public struct CharacterInfo
    {
        public string CharacterDisplayName;
        public uint? StationRecordKey;
    }

    /// <summary>
    /// Character selected in the console
    /// </summary>
    public uint? SelectedIndex { get; set; } = null;

    /// <summary>
    /// List of names+station record keys to display in the listing
    /// </summary>
    public Dictionary<uint, CharacterInfo>? CharacterList { get; set; }

    /// <summary>
    /// The contents of the selected record
    /// </summary>
    public FullCharacterRecords? SelectedRecord { get; set; } = null;

    public StationRecordsFilter? Filter { get; set; } = null;
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleFilterMsg : BoundUserInterfaceMessage
{
    public readonly StationRecordsFilter? Filter;

    public MedicalRecordsConsoleFilterMsg(StationRecordsFilter? filter)
    {
        Filter = filter;
    }
}

[Serializable, NetSerializable]
public sealed class MedicalRecordsConsoleSelectMsg : BoundUserInterfaceMessage
{
    public readonly uint? CharacterRecordKey;

    public MedicalRecordsConsoleSelectMsg(uint? medicalRecordsKey)
    {
        CharacterRecordKey = medicalRecordsKey;
    }
}
