using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Records;

[Prototype("medicalInfo"), Serializable]
public sealed partial class MedicalInfoPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; set; } = default!;

    /// <summary>
    /// The name of this information.
    /// </summary>
    [DataField]
    public LocId Name { get; set; } = string.Empty;

    /// <summary>
    /// Adds a trait to a category, allowing you to limit the selection of some traits to the settings of that category.
    /// </summary>
    [DataField]
    public ProtoId<MedicalInfoCategoryPrototype>? Category;
}
