using Content.Shared._NF.PlantAnalyzer;
using JetBrains.Annotations;

namespace Content.Client._NF.PlantAnalyzer.UI;

[UsedImplicitly]
public sealed class PlantAnalyzerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private PlantAnalyzerWindow? _window;

    public PlantAnalyzerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = new PlantAnalyzerWindow(this)
        {
            Title = Loc.GetString("plant-analyzer-interface-title"),
        };
        _window.OnClose += Close;
        _window.OpenCenteredLeft();
    }

    protected override void UpdateState(BoundUserInterfaceState state)  //Funkystation - Switched to state instead of message to fix UI bug
    {
        if (_window == null)
            return;

        if (state is not PlantAnalyzerScannedSeedPlantInformation cast)  //Funkystation - Switched to state instead of message to fix UI bug
            return;
        _window.Populate(cast);
    }

    public void AdvPressed(bool scanMode)
    {
        SendMessage(new PlantAnalyzerSetMode(scanMode));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;

        if (_window != null)
            _window.OnClose -= Close;

        _window?.Dispose();
    }
}
