using Content.Client._Funkystation.Printer.UI;
using Content.Shared._Funkystation.Printer;
using Content.Shared._Funkystation.Printer.Components;
using Robust.Client.UserInterface;
using Serilog;

namespace Content.Client._Funkystation.Printer;

public sealed class PrintingDeviceBoundUi(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private PrintingDeviceWindow? _menu;

    protected override void Open()
    {
        base.Open();
        
        _menu = this.CreateWindow<PrintingDeviceWindow>();

        _menu.TemplateSelectorPressed += Refresh;
        _menu.PrintButtonPressed += PrintButtonPressed;
    }
    
    public void Refresh()
    {
        var system = EntMan.System<PrintingDeviceSystem>();
        var templates = system.GetAvailableDocumentTemplates(Owner);
        
        _menu?.Populate(templates);

        if (_menu?.SelectedId != null)
        {
            _menu.PopulateFields(system.GetDocumentTemplateFields(_menu.SelectedId));
        }
    }

    public void PrintButtonPressed()
    {
        if (_menu?.SelectedId == null)
            return;

        var entries = _menu.GetAllFieldEntries();
        var selectedId = _menu.SelectedId;
        
        var msg = new PrintingDevicePrintRequestMessage(entries, selectedId);
        SendMessage(msg);
    }
}