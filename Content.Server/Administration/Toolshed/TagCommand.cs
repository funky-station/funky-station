// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 Tornado Tech <54727692+Tornado-Technology@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Linq;
using Content.Shared.Administration;
using Content.Shared.Tag;
using Robust.Shared.Prototypes;
using Robust.Shared.Toolshed;
using Robust.Shared.Toolshed.Syntax;
using Robust.Shared.Toolshed.TypeParsers;

namespace Content.Server.Administration.Toolshed;

[ToolshedCommand, AdminCommand(AdminFlags.Debug)]
public sealed class TagCommand : ToolshedCommand
{
    private TagSystem? _tag;

    [CommandImplementation("list")]
    public IEnumerable<ProtoId<TagPrototype>> List([PipedArgument] IEnumerable<EntityUid> ent)
    {
        return ent.SelectMany(x =>
        {
            if (TryComp<TagComponent>(x, out var tags))
                // Note: Cast is required for C# to figure out the type signature.
                return (IEnumerable<ProtoId<TagPrototype>>)tags.Tags;
            return Array.Empty<ProtoId<TagPrototype>>();
        });
    }

    [CommandImplementation("with")]
    public IEnumerable<EntityUid> With(
        [CommandInvocationContext] IInvocationContext ctx,
        [PipedArgument] IEnumerable<EntityUid> entities,
        [CommandArgument] ProtoId<TagPrototype> tag)
    {
        _tag ??= GetSys<TagSystem>();
        return entities.Where(e => _tag.HasTag(e, tag!));
    }

    [CommandImplementation("add")]
    public EntityUid Add([PipedArgument] EntityUid input, ProtoId<TagPrototype> tag)
    {
        _tag ??= GetSys<TagSystem>();
        _tag.AddTag(input, tag);
        return input;
    }

    [CommandImplementation("add")]
    public IEnumerable<EntityUid> Add([PipedArgument] IEnumerable<EntityUid> input, ProtoId<TagPrototype> tag)
        => input.Select(x => Add(x, tag));

    [CommandImplementation("rm")]
    public EntityUid Rm([PipedArgument] EntityUid input, ProtoId<TagPrototype> tag)
    {
        _tag ??= GetSys<TagSystem>();
        _tag.RemoveTag(input, tag);
        return input;
    }

    [CommandImplementation("rm")]
    public IEnumerable<EntityUid> Rm([PipedArgument] IEnumerable<EntityUid> input, ProtoId<TagPrototype> tag)
        => input.Select(x => Rm(x, tag));

    [CommandImplementation("addmany")]
    public EntityUid AddMany([PipedArgument] EntityUid input, IEnumerable<ProtoId<TagPrototype>> tags)
    {
        _tag ??= GetSys<TagSystem>();
        _tag.AddTags(input, tags);
        return input;
    }

    [CommandImplementation("addmany")]
    public IEnumerable<EntityUid> AddMany([PipedArgument] IEnumerable<EntityUid> input, IEnumerable<ProtoId<TagPrototype>> tags)
        => input.Select(x => AddMany(x, tags.ToArray()));

    [CommandImplementation("rmmany")]
    public EntityUid RmMany([PipedArgument] EntityUid input, IEnumerable<ProtoId<TagPrototype>> tags)
    {
        _tag ??= GetSys<TagSystem>();
        _tag.RemoveTags(input, tags);
        return input;
    }

    [CommandImplementation("rmmany")]
    public IEnumerable<EntityUid> RmMany([PipedArgument] IEnumerable<EntityUid> input, IEnumerable<ProtoId<TagPrototype>> tags)
        => input.Select(x => RmMany(x, tags.ToArray()));
}
