using Content.Shared.CriminalRecords;
using Content.Shared.StationRecords;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Payouts;

[Serializable, NetSerializable]
public enum PaymentRecordsConsoleKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class PayoutRecordsConsoleState(
    Dictionary<uint, string>? recordListing
    ) : BoundUserInterfaceState
{
    public uint? SelectedKey;
    public GeneralStationRecord? StationRecord;
    public PaymentRecord? PaymentRecord;
    public readonly Dictionary<uint, string>? RecordListing = recordListing;
    
    public bool IsEmpty() => SelectedKey == null && StationRecord == null && PaymentRecord == null && RecordListing == null;
}

/// <summary>
/// Used to change status, respecting the wanted/reason nullability rules in <see cref="CriminalRecord"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class PayoutRecordChangePay(string pay) : BoundUserInterfaceMessage
{
    public readonly string? Pay = pay;
}

/// <summary>
/// Used to change status, respecting the wanted/reason nullability rules in <see cref="CriminalRecord"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class PayoutRecordSuspendPay(bool suspended, string? reason) : BoundUserInterfaceMessage
{
    public readonly bool? Suspended = suspended;
    public readonly string? Reason = reason;
}

/// <summary>
/// Used to change status, respecting the wanted/reason nullability rules in <see cref="CriminalRecord"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class PayoutRecordGrantBonus(bool bonusAmount) : BoundUserInterfaceMessage
{
    public readonly bool? BonusAmount = bonusAmount;
}