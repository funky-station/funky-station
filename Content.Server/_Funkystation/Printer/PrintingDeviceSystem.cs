using System.Linq;
using Content.Shared._Funkystation.Printer;
using Content.Shared._Funkystation.Printer.Components;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Printer;

public sealed partial class PrintingDeviceSystem : SharedPrintingDeviceSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    
    public override void Initialize()
    {
        base.Initialize();
        
        SubscribeLocalEvent<PrintingDeviceComponent, MapInitEvent>(OnMapInit);
        
        Subs.BuiEvents<PrintingDeviceComponent>(PrintingDeviceUiKey.Key,
            subs =>
            {
                subs.Event<PrintingDevicePrintRequestMessage>(OnPrintingDeviceRequest);
            });
    }
    
    private void OnMapInit(EntityUid uid, PrintingDeviceComponent component, ref MapInitEvent args)
    {
        var documentTemplates = _prototypeManager.EnumeratePrototypes<DocumentTemplatePrototype>();
        
        // todo: add support for different types of printers having different documents
        foreach (var template in documentTemplates) 
            component.AvailableTemplates.Add(template);
    }

    private void OnPrintingDeviceRequest(Entity<PrintingDeviceComponent> ent, ref PrintingDevicePrintRequestMessage msg)
    {
        var template = _prototypeManager.Index<DocumentTemplatePrototype>(msg.Template.Id);
        
        // replace all template strings with those found in msg
        var text = msg.Data.Aggregate(template.Text, (current, dataEntry) => current.Replace($"[{dataEntry.Key}]", dataEntry.Value));

        var entXform = Transform(ent).Coordinates;

        var paper = SpawnAtPosition(template.PaperType, entXform);

        if (!TryComp<PaperComponent>(paper, out var comp))
            return;

        comp.Content = text;
    }
}