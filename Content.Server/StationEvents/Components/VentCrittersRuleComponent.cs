// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nim <128169402+Nimfar11@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 empty0set <16693552+empty0set@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 empty0set <empty0set@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.StationEvents.Events;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Storage;
using Robust.Shared.Map; // DeltaV

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(VentCrittersRule))]
public sealed partial class VentCrittersRuleComponent : Component
{
    // DeltaV: Replaced by Table
    //[DataField("entries")]
    //public List<EntitySpawnEntry> Entries = new();

    /// <summary>
    /// DeltaV: Table of possible entities to spawn.
    /// </summary>
    [DataField(required: true)]
    public EntityTableSelector Table = default!;

    /// <summary>
    /// At least one special entry is guaranteed to spawn
    /// </summary>
    [DataField("specialEntries")]
    public List<EntitySpawnEntry> SpecialEntries = new();

    /// <summary>
    /// DeltaV: The location of the vent that got picked.
    /// </summary>
    [ViewVariables]
    public EntityCoordinates? Location;

    /// <summary>
    /// DeltaV: Base minimum number of critters to spawn.
    /// </summary>
    [DataField]
    public int Min = 2;

    /// <summary>
    /// DeltaV: Base maximum number of critters to spawn.
    /// </summary>
    [DataField]
    public int Max = 3;

    /// <summary>
    /// DeltaV: Min and max get multiplied by the player count then divided by this.
    /// </summary>
    [DataField]
    public int PlayerRatio = 25;
}
