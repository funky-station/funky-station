// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 wrexbe <81056464+wrexbe@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2024 PrPleGoo <PrPleGoo@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Actions;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Client.Commands;

// Disabled until sandoxing issues are resolved. In the meantime, if you want to create an acttions preset, just disable
// sandboxing and uncomment this code (and the SaveActionAssignments() function).
/*
[AnyCommand]
public sealed class SaveActionsCommand : IConsoleCommand
{
    public string Command => "saveacts";
    public string Description => "Saves the current action toolbar assignments to a file";
    public string Help => $"Usage: {Command} <user resource path>";
    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        try
        {
            EntitySystem.Get<ActionsSystem>().SaveActionAssignments(args[0]);
        }
        catch
        {
            shell.WriteLine("Failed to save action assignments");
        }
    }
}
*/

[AnyCommand]
public sealed class LoadActionsCommand : LocalizedCommands
{
    [Dependency] private readonly IEntitySystemManager _entitySystemManager = default!;

    public override string Command => "loadacts";

    public override string Help => LocalizationManager.GetString($"cmd-{Command}-help", ("command", Command));

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteLine(Help);
            return;
        }

        try
        {
            _entitySystemManager.GetEntitySystem<ActionsSystem>().LoadActionAssignments(args[0], true);
        }
        catch
        {
            shell.WriteError(LocalizationManager.GetString($"cmd-{Command}-error"));
        }
    }
}
