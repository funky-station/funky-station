// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Moony <moony@hellomouse.net>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared.Magic.Events;

/// <summary>
///     Spell that uses the magic of ECS to add & remove components. Components are first removed, then added.
/// </summary>
public sealed partial class ChangeComponentsSpellEvent : EntityTargetActionEvent, ISpeakSpell
{
    // TODO allow it to set component data-fields?
    // for now a Hackish way to do that is to remove & add, but that doesn't allow you to selectively set specific data fields.

    [DataField]
    [AlwaysPushInheritance]
    public ComponentRegistry ToAdd = new();

    [DataField]
    [AlwaysPushInheritance]
    public HashSet<string> ToRemove = new();

    [DataField]
    public string? Speech { get; private set; }

    [DataField]
    public bool DoSpeech { get; private set; }
}
