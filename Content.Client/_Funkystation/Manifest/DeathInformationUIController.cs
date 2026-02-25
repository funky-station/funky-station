using Content.Shared._Funkystation.Manifest;
using OpenToolkit.GraphicsLibraryFramework;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Network;

namespace Content.Client._Funkystation.Manifest;

public sealed class DeathInformationUIController : UIController
{
    private DeathInformationWindow? _window;
    [Dependency] private readonly IClientNetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<DeathInfoOpenMessage>(OnDeathInfoOpen);
    }

    private void OnDeathInfoOpen(DeathInfoOpenMessage msg, EntitySessionEventArgs args)
    {
        _window?.Close();
        _window = new DeathInformationWindow();
        _window.OpenCentered();
        _window.OnClose += () => _window = null;
        _window.OnSubmitted += OnFeedbackSubmitted;
    }

    private void OnFeedbackSubmitted(string args)
    {
        var msg = new DeathInformationMessage { Description = args };
        _net.ClientSendMessage(msg);
        _window?.Close();
    }
}
