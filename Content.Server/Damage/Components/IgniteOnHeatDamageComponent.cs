// SPDX-FileCopyrightText: 2024 LankLTE <135308300+LankLTE@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server.Damage.Components;

[RegisterComponent]
public sealed partial class IgniteOnHeatDamageComponent : Component
{
    [DataField("fireStacks")]
    public float FireStacks = 1f;

    // The minimum amount of damage taken to apply fire stacks
    [DataField("threshold")]
    public FixedPoint2 Threshold = 15;
}
