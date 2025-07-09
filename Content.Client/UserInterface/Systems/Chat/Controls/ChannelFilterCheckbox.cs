using Content.Shared.Chat;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client.UserInterface.Systems.Chat.Controls;

public sealed class ChannelFilterCheckbox : CheckBox
{
    public readonly ChatChannel Channel;

    public bool IsHidden => Parent == null;

    public event Action<ChannelFilterCheckbox>? OnRightClicked;

    public ChannelFilterCheckbox(ChatChannel channel)
    {
        Channel = channel;
        Text = Loc.GetString($"hud-chatbox-channel-{Channel}");
    }

    private void UpdateText(int? unread)
    {
        var name = Loc.GetString($"hud-chatbox-channel-{Channel}");

        if (unread > 0)
            // todo: proper fluent stuff here.
            name += " (" + (unread > 9 ? "9+" : unread) + ")";

        Text = name;
    }

    public void UpdateUnreadCount(int? unread)
    {
        UpdateText(unread);
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        if (args.Function == EngineKeyFunctions.UIRightClick)
            OnRightClicked?.Invoke(this);
    }
}
