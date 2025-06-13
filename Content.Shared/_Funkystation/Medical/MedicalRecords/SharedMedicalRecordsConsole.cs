using Content.Shared._Funkystation.Records;
using Content.Shared.Security;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Medical.MedicalRecords;

// this needs to be redone bc it isnt predicted! yayyyy!
// copy-pasted from CD implementation but this isnt.... how this should be done.....
// most of this should be put into a comp,
// and then there should be a shared system folder to inherit from server
// or instead of inheriting from server, throw the server system stuff into shared to predict it?
// also combine the server comp file into shared
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
