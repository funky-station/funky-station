// SPDX-FileCopyrightText: 2021 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 nikthechampiongr <32041239+nikthechampiongr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Text;
using Content.Server.Administration.Managers;
using Content.Server.Afk;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Utility;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.AdminWho)]
public sealed class AdminWhoCommand : IConsoleCommand
{
    public string Command => "adminwho";
    public string Description => "Returns a list of all admins on the server";
    public string Help => "Usage: adminwho";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var adminMgr = IoCManager.Resolve<IAdminManager>();
        var afk = IoCManager.Resolve<IAfkManager>();

        var seeStealth = true;

        // If null it (hopefully) means it is being called from the console.
        if (shell.Player != null)
        {
            var playerData = adminMgr.GetAdminData(shell.Player);

            seeStealth = playerData != null && playerData.CanStealth();
        }

        var sb = new StringBuilder();
        var first = true;
        foreach (var admin in adminMgr.ActiveAdmins)
        {
            var adminData = adminMgr.GetAdminData(admin)!;
            DebugTools.AssertNotNull(adminData);

            if (adminData.Stealth && !seeStealth)
                continue;

            if (!first)
                sb.Append('\n');
            first = false;

            sb.Append(admin.Name);
            if (adminData.Title is { } title)
                sb.Append($": [{title}]");

            if (adminData.Stealth)
                sb.Append(" (S)");

            if (shell.Player is { } player && adminMgr.HasAdminFlag(player, AdminFlags.Admin))
            {
                if (afk.IsAfk(admin))
                    sb.Append(" [AFK]");
            }
        }

        shell.WriteLine(sb.ToString());
    }
}
