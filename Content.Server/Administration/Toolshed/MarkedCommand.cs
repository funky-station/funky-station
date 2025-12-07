// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Administration;
using Robust.Shared.Toolshed;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AnyCommand]
public sealed class MarkedCommand : ToolshedCommand
{
    [CommandImplementation]
    public IEnumerable<EntityUid> Marked(IInvocationContext ctx)
    {
        var marked = ctx.ReadVar("marked") as IEnumerable<EntityUid>;
        return marked ?? Array.Empty<EntityUid>();
    }
}
