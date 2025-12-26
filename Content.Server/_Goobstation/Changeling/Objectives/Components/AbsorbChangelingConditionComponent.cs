// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Marcus F <199992874+thebiggestbruh@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Server.Changeling.Objectives.Systems;
using Content.Server._Goobstation.Changeling.Objectives.Systems;

namespace Content.Server._Goobstation.Changeling.Objectives.Components;

[RegisterComponent, Access(typeof(ChangelingObjectiveSystem), typeof(Goobstation.Server.Changeling.ChangelingSystem))]
public sealed partial class AbsorbChangelingConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float LingAbsorbed = 0f;
}
