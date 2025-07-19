// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <comedian_vs_clown@hotmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

namespace Content.Shared.Eye.Blinding.Components;

/// <summary>
/// For welding masks, sunglasses, etc.
/// </summary>
[RegisterComponent]
public sealed partial class EyeProtectionComponent : Component
{
    /// <summary>
    /// How many seconds to subtract from the status effect. If it's greater than the source
    /// of blindness, do not blind.
    /// </summary>
    [DataField("protectionTime")]
    public TimeSpan ProtectionTime = TimeSpan.FromSeconds(10);
}
