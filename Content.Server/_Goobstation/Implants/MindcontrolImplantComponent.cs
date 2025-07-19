// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

namespace Content.Server.Implants.Components;

[RegisterComponent]
public sealed partial class MindcontrolImplantComponent : Component
{
    [DataField] public EntityUid? HolderUid = null; //who holds the implanter
    [DataField] public EntityUid? ImplanterUid = null; // the implanter carrying the implant
}
