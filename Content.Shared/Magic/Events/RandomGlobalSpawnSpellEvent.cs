// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Actions;
using Content.Shared.Storage;
using Robust.Shared.Audio;

namespace Content.Shared.Magic.Events;

public sealed partial class RandomGlobalSpawnSpellEvent : InstantActionEvent, ISpeakSpell
{
    /// <summary>
    /// The list of prototypes this spell can spawn, will select one randomly
    /// </summary>
    [DataField]
    public List<EntitySpawnEntry> Spawns = new();

    /// <summary>
    /// Sound that will play globally when cast
    /// </summary>
    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/staff_animation.ogg");

    [DataField]
    public string? Speech { get; private set; }

    /// <summary>
    /// Should this Global spawn spell turn its targets into a Survivor Antagonist?
    /// Ignores the caster for this.
    /// </summary>
    [DataField]
    public bool MakeSurvivorAntagonist = false;
}
