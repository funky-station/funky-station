// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.Heretic.Components;

[RegisterComponent]
public sealed partial class AristocratComponent : Component
{
    public float UpdateTimer = 0f;
    [DataField] public float UpdateDelay = 1.5f;
    [DataField] public float Range = 2.5f;
}
