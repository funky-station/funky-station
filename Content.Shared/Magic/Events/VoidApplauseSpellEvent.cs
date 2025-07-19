// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Magic.Events;

public sealed partial class VoidApplauseSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    [DataField]
    public string? Speech { get; private set; }

    /// <summary>
    ///     Emote to use.
    /// </summary>
    [DataField]
    public ProtoId<EmotePrototype> Emote = "ClapSingle";

    /// <summary>
    ///     Visual effect entity that is spawned at both the user's and the target's location.
    /// </summary>
    [DataField]
    public EntProtoId Effect = "EffectVoidBlink";
}
