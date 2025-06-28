using Content.Shared._Funkystation.Payouts;
using Content.Shared.Access.Systems;
using Content.Shared.StationRecords;
using Robust.Shared.Prototypes;

namespace Content.Client._Funkystation.Payouts;

public sealed class PaymentRecordsConsoleBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly AccessReaderSystem _accessReader;
    private PaymentRecordsConsoleWindow? _window;
    
    public PaymentRecordsConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _accessReader = EntMan.System<AccessReaderSystem>();
    }
    
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not PayoutRecordsConsoleState cast)
            return;

        _window?.UpdateState(cast);
    }

    protected override void Open()
    {
        base.Open();

        var comp = EntMan.GetComponent<PaymentRecordsConsoleComponent>(Owner);

        _window = new PaymentRecordsConsoleWindow(Owner, _proto);
        _window.OnKeySelected += key =>
            SendMessage(new SelectStationRecord(key));
        
        _window.OnClose += Close;
    }
}