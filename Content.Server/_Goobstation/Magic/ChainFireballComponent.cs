// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.Magic;

[RegisterComponent]
public sealed partial class ChainFireballComponent : Component
{
    /// <summary>
    ///     The chance of the ball disappearing (in %)
    /// </summary>
    [DataField] public float DisappearChance = 0.05f;

    public List<EntityUid> IgnoredTargets = new();
}
