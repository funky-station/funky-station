using Content.Shared._Funkystation.Printer.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Funkystation.Printer;

public abstract class SharedPrintingDeviceSystem : EntitySystem
{
    
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PrintingDeviceComponent, MapInitEvent>(OnMapInit);
    }
    
    private void OnMapInit(EntityUid uid, PrintingDeviceComponent component, ref MapInitEvent args)
    {
        var documentTemplates = _prototypeManager.EnumeratePrototypes<DocumentTemplatePrototype>();
        
        // todo: add support for different types of printers having different documents
        foreach (var template in documentTemplates) 
            component.AvailableTemplates.Add(template);
        
        Dirty(uid, component);
    }

    public List<DocumentTemplatePrototype> GetAvailableDocumentTemplates(EntityUid uid, 
        PrintingDeviceComponent? printingDeviceComponent = null)
    {
        return !Resolve(uid, ref printingDeviceComponent) ? [] : printingDeviceComponent.AvailableTemplates;
    }

    public Dictionary<string, string> GetDocumentTemplateFields(string id)
    {
        return _prototypeManager.Index<DocumentTemplatePrototype>(id).Fields;
    }
}