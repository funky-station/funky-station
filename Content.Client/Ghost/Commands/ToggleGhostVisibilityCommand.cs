// SPDX-FileCopyrightText: 2024 ShadowCommander <shadowjjt@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Console;

namespace Content.Client.Ghost.Commands;

public sealed class ToggleGhostVisibilityCommand : IConsoleCommand
{
    [Dependency] private readonly IEntitySystemManager _entSysMan = default!;

    public string Command => "toggleghostvisibility";
    public string Description => "Toggles ghost visibility on the client.";
    public string Help => "toggleghostvisibility [bool]";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var ghostSystem = _entSysMan.GetEntitySystem<GhostSystem>();

        if (args.Length != 0 && bool.TryParse(args[0], out var visibility))
        {
            ghostSystem.ToggleGhostVisibility(visibility);
        }
        else
        {
            ghostSystem.ToggleGhostVisibility();
        }
    }
}
