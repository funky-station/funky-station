using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Economy;

[DataDefinition]
[Prototype("paymentSalary")]
public sealed partial class PaymentSalaryPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The roles that has this applied to their salary.
    /// </summary>
    [DataField] public List<string> Roles = [];

    [DataField] public int Salary = 0;
}