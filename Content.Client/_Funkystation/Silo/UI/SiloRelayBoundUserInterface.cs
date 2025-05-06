using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Funkystation.Silo.UI;

[UsedImplicitly]
public sealed class SiloRelayBoundUserInterface : BoundUserInterface
{

    [ViewVariables]
    private SiloRelayMenu? _menu;

    public SiloRelayBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindowCenteredRight<SiloRelayMenu>();
        _menu.SetEntity(Owner);
    }
}
