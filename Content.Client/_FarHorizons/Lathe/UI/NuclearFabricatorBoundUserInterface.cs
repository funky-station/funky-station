using Content.Shared._FarHorizons.Lathe;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._FarHorizons.Lathe.UI;

/// <summary>
/// Initializes a <see cref="NuclearFabricatorWindow"/> and updates it when new server messages are received.
/// </summary>
[UsedImplicitly]
public sealed class NuclearFabricatorBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private NuclearFabricatorWindow? _window;

    public NuclearFabricatorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<NuclearFabricatorWindow>();

        Update();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NuclearFabricatorBuiState reactorState)
            return;

        _window?.Update(reactorState);
    }
}