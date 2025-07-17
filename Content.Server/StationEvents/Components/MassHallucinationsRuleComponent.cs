// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Slava0135 <40753025+Slava0135@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.StationEvents.Events;
using Robust.Shared.Audio;
using Robust.Shared.Collections;

namespace Content.Server.StationEvents.Components;

[RegisterComponent, Access(typeof(MassHallucinationsRule))]
public sealed partial class MassHallucinationsRuleComponent : Component
{
    /// <summary>
    /// The maximum time between incidents in seconds
    /// </summary>
    [DataField("maxTimeBetweenIncidents", required: true), ViewVariables(VVAccess.ReadWrite)]
    public float MaxTimeBetweenIncidents;

    /// <summary>
    /// The minimum time between incidents in seconds
    /// </summary>
    [DataField("minTimeBetweenIncidents", required: true), ViewVariables(VVAccess.ReadWrite)]
    public float MinTimeBetweenIncidents;

    [DataField("maxSoundDistance", required: true), ViewVariables(VVAccess.ReadWrite)]
    public float MaxSoundDistance;

    [DataField("sounds", required: true)]
    public SoundSpecifier Sounds = default!;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<EntityUid> AffectedEntities = new();
}
