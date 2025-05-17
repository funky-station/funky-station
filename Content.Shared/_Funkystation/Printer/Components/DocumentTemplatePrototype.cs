using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Funkystation.Printer.Components;

[Serializable, DataDefinition]
[Prototype("printerDocumentTemplate")]
public sealed partial class DocumentTemplatePrototype : IPrototype
{
    [IdDataField] 
    public string ID { get; private set; } = default!;
    
    [DataField]
    public string TemplateName { get; set; }
    
    [DataField]
    public Dictionary<string, string> Fields { get; set; }
    
    [DataField]
    public EntProtoId PaperType { get; set; }
    
    [DataField]
    public string Text { get; set; }
}
