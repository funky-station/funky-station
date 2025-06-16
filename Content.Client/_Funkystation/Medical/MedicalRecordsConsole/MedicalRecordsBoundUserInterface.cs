using Content.Client._Funkystation.Medical.MedicalRecordsConsole.UI;
using Content.Shared._Funkystation.Medical.MedicalRecords;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Medical.MedicalRecordsConsole;

[UsedImplicitly]
public sealed class MedicalRecordsBoundUserInterface(EntityUid owner, Enum uiKey): BoundUserInterface(owner, uiKey)
{
    [ViewVariables] private MedicalRecordsMenu? _menu;

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not MedicalRecordsConsoleState state)
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
