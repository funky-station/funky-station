using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Records;

[Prototype("medicalInfoCategory"), Serializable]
public sealed partial class MedicalInfoCategoryPrototype : IPrototype
{
    public const string Default = "Default";

    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Name of the category displayed in the UI
    /// </summary>
    [DataField]
    public LocId Name { get; private set; } = string.Empty;
}
