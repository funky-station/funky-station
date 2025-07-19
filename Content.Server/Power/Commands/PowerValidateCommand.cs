// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration;
using Content.Server.Power.EntitySystems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Power.Commands;

[AdminCommand(AdminFlags.Debug)]
public sealed class PowerValidateCommand : LocalizedEntityCommands
{
    [Dependency] private readonly PowerNetSystem _powerNet = null!;

    public override string Command => "power_validate";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        try
        {
            _powerNet.Validate();
        }
        catch (Exception e)
        {
            shell.WriteLine(LocalizationManager.GetString("cmd-power_validate-error", ("err", e.ToString())));
            return;
        }

        shell.WriteLine(LocalizationManager.GetString("cmd-power_validate-success"));
    }
}
