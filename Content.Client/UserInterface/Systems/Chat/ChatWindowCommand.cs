// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using JetBrains.Annotations;
using Robust.Shared.Console;

namespace Content.Client.UserInterface.Systems.Chat;

/// <summary>
/// Command which creates a window containing a chatbox
/// </summary>
[UsedImplicitly]
public sealed class ChatWindowCommand : LocalizedCommands
{
    public override string Command => "chatwindow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var window = new ChatWindow();
        window.OpenCentered();
    }
}

/// <summary>
/// Command which creates a window containing a chatbox configured for admin use
/// </summary>
[UsedImplicitly]
public sealed class AdminChatWindowCommand : LocalizedCommands
{
    public override string Command => "achatwindow";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var window = new ChatWindow();
        window.ConfigureForAdminChat();
        window.OpenCentered();
    }
}
