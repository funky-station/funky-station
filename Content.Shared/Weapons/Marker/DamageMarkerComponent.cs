// SPDX-FileCopyrightText: 2023 AJCM-git <60196617+AJCM-git@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Marker;

/// <summary>
/// Marks an entity to take additional damage
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedDamageMarkerSystem))]
[AutoGenerateComponentPause]
public sealed partial class DamageMarkerComponent : Component
{
    /// <summary>
    /// Sprite to apply to the entity while damagemarker is applied.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("effect")]
    public SpriteSpecifier.Rsi? Effect = new(new ResPath("/Textures/Objects/Weapons/Effects"), "shield2");

    /// <summary>
    /// Sound to play when the damage marker is procced.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("sound")]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/Weapons/Guns/Gunshots/kinetic_accel.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("damage")]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Entity that marked this entity for a damage surplus.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("marker"), AutoNetworkedField]
    public EntityUid Marker;

    [ViewVariables(VVAccess.ReadWrite), DataField("endTime", customTypeSerializer:typeof(TimeOffsetSerializer)), AutoNetworkedField]
    [AutoPausedField]
    public TimeSpan EndTime;
}
