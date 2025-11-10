// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Component for Cosmic Cult's Vacuous Chantry.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicChantryComponent : Component
{
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan SpawnTimer = default!;

    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CountdownTimer = default!;

    [DataField] public TimeSpan SpawningTime = TimeSpan.FromSeconds(2.4);

    [DataField] public TimeSpan EventTime = TimeSpan.FromSeconds(150);

    [DataField] public bool Spawned;

    [DataField] public bool Completed;

    [DataField] public EntityUid PolyVictim;

    [DataField] public EntityUid Victim;

    [DataField] public SoundSpecifier ChantryAlarm = new SoundPathSpecifier("/Audio/_DV/CosmicCult/chantry_alarm.ogg");

    [DataField] public SoundSpecifier SpawnSFX = new SoundPathSpecifier("/Audio/_DV/CosmicCult/colossus_spawn.ogg");

    [DataField] public EntProtoId Colossus = "MobCosmicColossus";

    [DataField] public EntProtoId SpawnVFX = "CosmicGlareAbilityVFX";
}

[Serializable, NetSerializable]
public enum ChantryVisuals : byte
{
    Status,
}

[Serializable, NetSerializable]
public enum ChantryStatus : byte
{
    Off,
    On,
}
