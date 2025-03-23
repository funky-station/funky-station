using Robust.Shared.Serialization;

namespace Content.Server._Funkystation.Payouts;

[Serializable, DataRecord]
public sealed class PaymentRecord
{
    [DataField]
    public bool PaySuspended = false;

    [DataField]
    public int PayoutAmount = 0;

    [DataField]
    public List<PayoutReceipt> History = new();
}

[Serializable]
public record struct PayoutReceipt(TimeSpan PayoutTime, bool Paid, int Amount);
