// SPDX-FileCopyrightText: 2022 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 metalgearsloth <metalgearsloth@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Weapons.Ranged.Systems;
using Robust.Shared.Console;

namespace Content.Client.Weapons.Ranged.Commands;

public sealed class ShowSpreadCommand : LocalizedEntityCommands
{
    [Dependency] private readonly GunSystem _gunSystem = default!;

    public override string Command => "showgunspread";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        _gunSystem.SpreadOverlay ^= true;

        shell.WriteLine(Loc.GetString($"cmd-showgunspread-status", ("status", _gunSystem.SpreadOverlay)));
    }
}
