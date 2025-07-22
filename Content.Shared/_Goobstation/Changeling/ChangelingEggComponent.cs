// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 yglop <95057024+yglop@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.Mind;
using Content.Shared.Store.Components;
using Robust.Shared.GameStates;

namespace Content.Shared.Changeling;

[RegisterComponent, NetworkedComponent]

public sealed partial class ChangelingEggComponent : Component
{
    public ChangelingComponent lingComp;
    public EntityUid lingMind;
    public StoreComponent lingStore;

    /// <summary>
    ///     Countdown before spawning monkey.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 120f;
    public bool active = false;
}
