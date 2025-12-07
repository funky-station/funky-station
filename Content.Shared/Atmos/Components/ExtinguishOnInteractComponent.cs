// SPDX-FileCopyrightText: 2023 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 faint <46868845+ficcialfaint@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Aiden <aiden@djkraz.com>
// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Robust.Shared.Audio;

namespace Content.Shared.Atmos.Components;
/// <summary>
/// Allows you to extinguish an object by interacting with it
/// </summary>
[RegisterComponent]
public sealed partial class ExtinguishOnInteractComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public SoundSpecifier? ExtinguishAttemptSound = new SoundPathSpecifier("/Audio/Items/candle_blowing.ogg");

    /// <summary>
    /// Extinguishing chance
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float Probability = 0.9f;

    /// <summary>
    /// Number of fire stacks to be changed on successful interaction.
    /// </summary>
    // With positive values, the interaction will conversely fan the fire,
    // which is useful for any blacksmithing mechs
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float StackDelta = -5.0f;

    [DataField]
    public LocId ExtinguishFailed = "candle-extinguish-failed";
}

public sealed partial class ResistFireAlertEvent : BaseAlertEvent;
