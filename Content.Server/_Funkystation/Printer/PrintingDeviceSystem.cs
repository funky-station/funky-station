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
        
        Subs.BuiEvents<PrintingDeviceComponent>(PrintingDeviceUiKey.Key,
            subs =>
            {
                subs.Event<PrintingDevicePrintRequestMessage>(OnPrintingDeviceRequest);
            });
    }

    private void OnPrintingDeviceRequest(Entity<PrintingDeviceComponent> ent, ref PrintingDevicePrintRequestMessage msg)
    {
        var template = _prototypeManager.Index<DocumentTemplatePrototype>(msg.Template.Id);
        var text = template.Text;
        
        // replace all template strings with those found in msg
        foreach (var val in msg.Data)
        { 
            text = text.Replace($"$({val.Key})", val.Value);
        }
        
        Log.Debug(msg.Data.ToString()!);
        
        var entXform = Transform(ent).Coordinates;

        var paper = SpawnAtPosition(template.PaperType, entXform);

        if (!TryComp<PaperComponent>(paper, out var comp))
            return;

        comp.Content = text;
    }
}