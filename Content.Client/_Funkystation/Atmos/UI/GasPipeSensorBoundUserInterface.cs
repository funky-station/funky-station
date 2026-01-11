using Content.Shared.Atmos.Components;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Atmos.UI;

public sealed class GasPipeSensorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GasPipeSensorWindow? _window;

    public GasPipeSensorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindowCenteredLeft<GasPipeSensorWindow>();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (_window == null)
            return;

        if (message is not GasPipeSensorUserMessage msg)
            return;

        _window.Populate(msg);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _window?.Dispose();
    }
}
