namespace Content.Shared._Funkystation.Economy.StationRecords;

[Serializable, DataRecord]
public sealed class PaymentRecord
{
    [DataField] public bool PaySuspended = false;

    [DataField] public int PayoutAmount = 0;

    [DataField] public List<PayoutReceipt> History = [];
}

[Serializable]
public record struct PayoutReceipt(TimeSpan PayoutTime, bool Paid, int Amount);