// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Sirionaut <148076704+Sirionaut@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 MilenVolf <63782763+MilenVolf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Animals.Systems;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Animals.Components;

/// <summary>
///     This component handles animals which lay eggs (or some other item) on a timer, using up hunger to do so.
///     It also grants an action to players who are controlling these entities, allowing them to do it manually.
/// </summary>

[RegisterComponent, Access(typeof(EggLayerSystem)), AutoGenerateComponentPause]
public sealed partial class EggLayerComponent : Component
{
    /// <summary>
    ///     The item that gets laid/spawned, retrieved from animal prototype.
    /// </summary>
    [DataField(required: true)]
    public List<EntitySpawnEntry> EggSpawn = new();

    /// <summary>
    ///     Player action.
    /// </summary>
    [DataField]
    public EntProtoId EggLayAction = "ActionAnimalLayEgg";

    [DataField]
    public SoundSpecifier EggLaySound = new SoundPathSpecifier("/Audio/Effects/pop.ogg");

    /// <summary>
    ///     Minimum cooldown used for the automatic egg laying.
    /// </summary>
    [DataField]
    public float EggLayCooldownMin = 60f;

    /// <summary>
    ///     Maximum cooldown used for the automatic egg laying.
    /// </summary>
    [DataField]
    public float EggLayCooldownMax = 120f;

    /// <summary>
    ///     The amount of nutrient consumed on update.
    /// </summary>
    [DataField]
    public float HungerUsage = 60f;

    [DataField] public EntityUid? Action;

    /// <summary>
    ///     When to next try to produce.
    /// </summary>
    [DataField, AutoPausedField]
    public TimeSpan NextGrowth = TimeSpan.Zero;
}
