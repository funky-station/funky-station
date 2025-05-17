using Content.Client._Funkystation.Printer.UI;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Printer;

public sealed class PrintingDeviceBoundUi(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [ViewVariables]
    private PrintingDeviceMenu? _menu;

    protected override void Open()
    {
        base.Open();
        
        _menu = this.CreateWindow<PrintingDeviceMenu>();
    }
}