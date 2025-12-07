// SPDX-FileCopyrightText: 2024 Ed <96445749+TheShuEd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GreyMario <mariomister541@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Shared.Audio;

namespace Content.Server.Containers;

/// <summary>
/// Allows objects to fall inside the Container when thrown
/// </summary>
[RegisterComponent]
[Access(typeof(ThrowInsertContainerSystem))]
public sealed partial class ThrowInsertContainerComponent : Component
{
    [DataField(required: true)]
    public string ContainerId = string.Empty;

    /// <summary>
    /// Throw chance of hitting into the container
    /// </summary>
    [DataField]
    public float Probability = 0.5f; // funkystation: 0.75f;

    /// <summary>
    /// Sound played when an object is throw into the container.
    /// </summary>
    [DataField]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Effects/trashbag1.ogg");

    /// <summary>
    /// Sound played when an item is thrown and misses the container.
    /// </summary>
    [DataField]
    public SoundSpecifier? MissSound = new SoundPathSpecifier("/Audio/Effects/thudswoosh.ogg");

    [DataField]
    public LocId MissLocString = "container-thrown-missed";
}
