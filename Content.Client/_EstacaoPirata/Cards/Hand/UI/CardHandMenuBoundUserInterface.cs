using Content.Client.UserInterface.Controls;
using Content.Shared._EstacaoPirata.Cards.Hand;
using Content.Shared.RCD;
using JetBrains.Annotations;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Prototypes;

namespace Content.Client._EstacaoPirata.Cards.Hand.UI;

[UsedImplicitly]
public sealed class CardHandMenuBoundUserInterface : BoundUserInterface
{
    [Dependency] private readonly IClyde _displayManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private SimpleRadialMenu? _menu;

    public CardHandMenuBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        // SimpleRadialMenu has a parameterless ctor and Track(...) to attach to an entity.
        _menu = new SimpleRadialMenu();
        _menu.Track(Owner);
        _menu.OnClose += Close;

        // Open the menu centered on the mouse using the SimpleRadialMenu helper.
        _menu.OpenOverMouseScreenPosition();
    }

    public void SendCardHandDrawMessage(NetEntity e)
    {
        SendMessage(new CardHandDrawMessage(e));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _menu?.Dispose();
    }
}
