using Content.Client._Funkystation.Medical.MedicalRecordsConsole.UI;
using Content.Shared._Funkystation.Medical.MedicalRecords;
using Content.Shared.Access.Systems;
using Content.Shared.StationRecords;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Medical.MedicalRecordsConsole;

[UsedImplicitly]
public sealed class MedicalRecordsBoundUserInterface : BoundUserInterface
{
    [ViewVariables] private MedicalRecordsMenu? _menu;
    private readonly AccessReaderSystem _accessReader;

    public MedicalRecordsBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _accessReader = EntMan.System<AccessReaderSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<MedicalRecordsMenu>();
        _menu.OnClose += Close;

        _menu.OnListingItemSelected += meta =>
        {
            SendMessage(new MedicalRecordsConsoleSelectMsg(meta?.CharacterRecordKey));
        };

        _menu.OnFiltersChanged += (ty, txt) =>
        {
            SendMessage(txt == null
                ? new MedicalRecordsConsoleFilterMsg(null)
                : new MedicalRecordsConsoleFilterMsg(new StationRecordsFilter(ty, txt)));
        };

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not MedicalRecordsConsoleState cast)
            return;

        _menu?.UpdateState(cast);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _menu?.Close();
    }

}
