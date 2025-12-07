// SPDX-FileCopyrightText: 2024 Fishbait <Fishbait@git.ml>
// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Robust.Shared.Audio;

namespace Content.Server._EinsteinEngines.Silicon;

/// <summary>
///     Applies a <see cref="SpamEmitSoundComponent"/> to a Silicon when its battery is drained, and removes it when it's not.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconEmitSoundOnDrainedComponent : Component
{
    [DataField]
    public SoundSpecifier Sound = default!;

    [DataField]
    public TimeSpan MinInterval = TimeSpan.FromSeconds(8);

    [DataField]
    public TimeSpan MaxInterval = TimeSpan.FromSeconds(15);

    [DataField]
    public float PlayChance = 1f;

    [DataField]
    public string? PopUp;
}
