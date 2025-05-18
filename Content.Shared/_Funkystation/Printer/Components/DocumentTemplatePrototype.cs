using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Printer.Components;

[Serializable, DataDefinition]
[Prototype("printerDocumentTemplate")]
public sealed partial class DocumentTemplatePrototype : IPrototype
{
    [IdDataField] 
    public string ID { get; private set; } = default!;

    [DataField] 
    public string TemplateName;

    [DataField] 
    public Dictionary<string, string> Fields;

    [DataField] 
    public EntProtoId PaperType;

    [DataField] 
    public string Text;
}
