// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Kandiyaki <106633914+Kandiyaki@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Heretic;
using Robust.Shared.Audio;

namespace Content.Server.Magic;

[RegisterComponent]
public sealed partial class ImmovableVoidRodComponent : Component
{
    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(1f);

    public float Accumulator = 0f;

    [DataField]
    public string SnowWallPrototype = "WallIce";

    [DataField]
    public string IceTilePrototype = "FloorAstroIce";

    [NonSerialized] public Entity<HereticComponent>? User = null;
}
