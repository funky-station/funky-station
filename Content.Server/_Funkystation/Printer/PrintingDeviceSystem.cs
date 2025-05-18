using System.Linq;
using Content.Shared._Funkystation.Printer;
using Content.Shared._Funkystation.Printer.Components;
using Content.Shared.Paper;
using Robust.Shared.Prototypes;

namespace Content.Server._Funkystation.Printer;

public sealed partial class PrintingDeviceSystem : SharedPrintingDeviceSystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly PaperSystem _paperSystem = default!;
    
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
        
        // replace all template strings with those found in msg
        var text = msg.Data.Aggregate(Loc.GetString(template.Text), (current, val) => current.Replace($"$({val.Key})", val.Value));
        
        var entXform = Transform(ent).Coordinates;
        var paper = SpawnAtPosition(template.PaperType, entXform);
        
        if (!TryComp<PaperComponent>(paper, out var comp))
            return;
        
        _paperSystem.SetContent((paper, comp), text);
    }
}