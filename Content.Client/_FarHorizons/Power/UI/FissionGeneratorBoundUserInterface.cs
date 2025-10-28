using Content.Client.UserInterface;
using Content.Shared._FarHorizons.Power.Generation.FissionGenerator;
using JetBrains.Annotations;
using Robust.Client.Timing;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Power.UI;

/// <summary>
/// Initializes a <see cref="FissionGeneratorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class FisisonGeneratorBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private FissionGeneratorWindow? _window;

    public FisisonGeneratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<FissionGeneratorWindow>();

        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not FissionGeneratorBuiState reactorState)
            return;

        _window?.Update(reactorState);
    }
}