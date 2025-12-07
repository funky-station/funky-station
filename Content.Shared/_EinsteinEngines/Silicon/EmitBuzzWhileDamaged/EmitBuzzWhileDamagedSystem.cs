// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.ComponentModel.DataAnnotations;
using Robust.Shared.Audio;

namespace Content.Shared._EinsteinEngines.Silicon.EmitBuzzWhileDamaged;

/// <summary>
/// This is used for controlling the cadence of the buzzing emitted by EmitBuzzOnCritSystem.
/// This component is used by mechanical species that can get to critical health.
/// </summary>
[RegisterComponent]
public sealed partial class EmitBuzzWhileDamagedComponent : Component
{
    [DataField("buzzPopupCooldown")]
    public TimeSpan BuzzPopupCooldown { get; private set; } = TimeSpan.FromSeconds(8);

    [ViewVariables]
    public TimeSpan LastBuzzPopupTime;

    [DataField("cycleDelay")]
    public float CycleDelay = 2.0f;

    public float AccumulatedFrametime;

    [DataField("sound")]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("buzzes");
}
