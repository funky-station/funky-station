using Content.Client._Funkystation.Medical.MedicalRecordsConsole.UI;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Medical.MedicalRecords;

public sealed class MedicalRecordsBoundUserInterface: BoundUserInterface
{
    [ViewVariables]
    private MedicalRecordsMenu? _menu;

    public MedicalRecordsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }
    // this file is how to handle when the ui is open, and the classes for updating it

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MedicalRecordsMenu>();
        _menu.OpenCentered();
        _menu.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {

    }
}
