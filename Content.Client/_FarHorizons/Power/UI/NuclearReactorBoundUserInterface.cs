using System.Numerics;
using Content.Client.UserInterface;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using Content.Shared.Atmos.Piping.Binary.Components;
using Content.Shared.Atmos.Piping.Unary.Components;
using Content.Shared.IdentityManagement;
using JetBrains.Annotations;
using Robust.Client.Timing;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="NuclearReactorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class NuclearReactorBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private NuclearReactorWindow? _window;

    public NuclearReactorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<NuclearReactorWindow>();

        _window.ItemActionButtonPressed += OnActionButtonPressed;
        _window.EjectButtonPressed += OnEjectButtonPressed;

        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NuclearReactorBuiState reactorState || !EntMan.TryGetComponent(Owner, out NuclearReactorComponent? component))
            return;

        //var item = component.SlottedItem;
        //var partLabel = item != null ? Identity.Name((EntityUid)item, EntMan) : null;
        //_window?.SetItemName(partLabel);

        _window?.Update(reactorState);
    }

    protected void OnActionButtonPressed(Vector2d vector)
    {
        if (_window is null ) return;

        SendPredictedMessage(new ReactorItemActionMessage(vector));
    }

    protected void OnEjectButtonPressed()
    {
        if (_window is null) return;

        SendPredictedMessage(new ReactorEjectItemMessage());
    }
}