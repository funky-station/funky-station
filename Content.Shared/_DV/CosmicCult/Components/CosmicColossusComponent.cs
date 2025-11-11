// SPDX-FileCopyrightText: 2025 AftrLite <61218133+AftrLite@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 No Elka <no.elka.the.god@gmail.com>
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
/// Component for Cosmic Cult's entropic colossus.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentPause]
public sealed partial class CosmicColossusComponent : Component
{
    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan AttackHoldTimer = default!;

    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan HibernationTimer = default!;

    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan DeathTimer = default!;

    [AutoPausedField, DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan TeleportTimer = default!; // Funky

    [DataField] public SoundSpecifier ReawakenSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/colossus_spawn.ogg");

    [DataField] public SoundSpecifier DeathSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/colossus_death.ogg");

    [DataField] public SoundSpecifier IngressSfx = new SoundPathSpecifier("/Audio/_DV/CosmicCult/ability_ingress.ogg");

    [DataField] public SoundSpecifier DoAfterSfx = new SoundPathSpecifier("/Audio/Machines/airlock_creaking.ogg");

    [DataField] public EntProtoId CultVfx = "CosmicGenericVFX";

    [DataField] public EntProtoId CultBigVfx = "CosmicGlareAbilityVFX";

    [DataField] public EntProtoId Attack1Vfx = "CosmicColossusAttack1Vfx";

    [DataField] public EntProtoId TileDetonations = "MobTileDamageZone";

    [DataField] public EntProtoId EffigyPrototype = "CosmicEffigy";

    [DataField] public EntProtoId EffigyObjective = "ColossusEffigyObjective";

    [DataField] public EntProtoId EffigyPlaceAction = "ActionCosmicColossusEffigy";

    [DataField] public EntityUid? EffigyPlaceActionEntity;

    [DataField] public TimeSpan IngressDoAfter = TimeSpan.FromSeconds(4);

    [DataField] public TimeSpan AttackWait = TimeSpan.FromSeconds(1.5);

    [DataField] public TimeSpan HibernationWait = TimeSpan.FromSeconds(30); // Funky, was 20

    [DataField] public TimeSpan DeathWait = TimeSpan.FromMinutes(15);

    [DataField] public TimeSpan TeleportWait = TimeSpan.FromSeconds(1.5); // Funky

    [DataField] public bool Attacking;

    [DataField] public bool Hibernating;

    [DataField] public bool Timed;

    [DataField] public bool Teleporting; // Funky
}

[Serializable, NetSerializable]
public enum ColossusVisuals : byte
{
    Status,
    Hibernation,
    Sunder,
}

[Serializable, NetSerializable]
public enum ColossusStatus : byte
{
    Alive,
    Dead,
    Action,
}

[Serializable, NetSerializable]
public enum ColossusAction : byte
{
    Running,
    Stopped,
}
