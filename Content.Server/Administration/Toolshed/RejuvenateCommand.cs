// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Administration.Systems;
using Content.Shared.Administration;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Errors;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Debug)]
public sealed class RejuvenateCommand : ToolshedCommand
{
    private RejuvenateSystem? _rejuvenate;

    [CommandImplementation]
    public IEnumerable<EntityUid> Rejuvenate([PipedArgument] IEnumerable<EntityUid> input)
    {
        _rejuvenate ??= GetSys<RejuvenateSystem>();

        foreach (var i in input)
        {
            _rejuvenate.PerformRejuvenate(i);
            yield return i;
        }
    }

    [CommandImplementation]
    public void Rejuvenate(IInvocationContext ctx)
    {
        _rejuvenate ??= GetSys<RejuvenateSystem>();
        if (ExecutingEntity(ctx) is not { } ent)
        {
            if (ctx.Session is {} session)
                ctx.ReportError(new SessionHasNoEntityError(session));
            else
                ctx.ReportError(new NotForServerConsoleError());
        }
        else
            _rejuvenate.PerformRejuvenate(ent);
    }
}
