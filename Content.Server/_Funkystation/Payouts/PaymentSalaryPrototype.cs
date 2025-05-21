using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Payouts.Prototypes;

[Serializable, DataDefinition]
[Prototype("paymentSalary")]
public sealed partial class PaymentSalaryPrototype : IPrototype
{
    [IdDataField] public string ID { get; private set; } = default!;

    /// <summary>
    /// The roles that has this applied to their salary.
    /// </summary>
    [DataField("roles")] public List<JobPrototype> Roles = new List<JobPrototype>();

    [DataField("salary")] public int Salary = 0;
}
