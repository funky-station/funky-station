namespace Content.Shared._Funkystation.Payouts;

[RegisterComponent]
public sealed partial class PaymentRecordsConsoleComponent : Component
{
    [DataField]
    public uint? ActiveKey;
    
    /// <summary>
    /// Max length of reason string
    /// </summary>
    [DataField]
    public uint MaxStringLength = 30;
}