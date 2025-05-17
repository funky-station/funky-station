using Content.Client.UserInterface.Controls;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client._Funkystation.Printer.UI;

public sealed class PrintingDeviceMenu : BaseWindow
{
    public PrintingDeviceMenu()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);
        
        
    }
}