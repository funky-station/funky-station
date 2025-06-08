using Content.Client._Funkystation.Medical.MedicalRecordsConsole.UI;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Medical.MedicalRecords;

[UsedImplicitly]
public sealed class MedicalRecordsBoundUserInterface(EntityUid owner, Enum uiKey): BoundUserInterface(owner, uiKey)
{
    [ViewVariables] private MedicalRecordsMenu? _menu;

    // this file is how to handle when the ui is open, and the classes for updating it

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MedicalRecordsState state)
            return;

        _menu?.UpdateState(state);
    }
    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MedicalRecordsMenu>();
        _menu.OnClose += Close;

        _menu.OnListingSelected += meta =>
        {
            SendMessage(new MedicalRecordsConsoleSelectMsg(meta?.MedicalRecordsKey));
        };

        _menu.OnFiltersChanges += (ty, text) =>
        {
            SendMessage(txt == null
                ? new MedicalRecordsConsoleFilterMsg(null)
                : new MedicalRecordsConsoleFilterMsg(new MedicalRecordsConsoleFilterMsg(ty, txt)));
        };

        _menu.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _menu?.Close();
    }

}
