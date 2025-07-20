// SPDX-FileCopyrightText: 2024 BombasterDS <115770678+BombasterDS@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server._Goobstation.ChronoLegionnaire;

/// <summary>
/// Marks projectiles that will apply stasis on hit
/// </summary>
[RegisterComponent, Access(typeof(StasisOnCollideSystem))]
public sealed partial class StasisOnCollideComponent : Component
{
    [DataField("stasisTime")]
    public TimeSpan StasisTime = TimeSpan.FromSeconds(60);

    [DataField("fixture")]
    public string FixtureID = "projectile";
}
