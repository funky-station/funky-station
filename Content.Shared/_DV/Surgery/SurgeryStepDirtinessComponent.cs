// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 deltanedas <@deltanedas:kde.org>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._DV.Surgery;

/// <summary>
///     Component that allows a step to increase tools and gloves' dirtiness
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryStepDirtinessComponent : Component
{
    /// <summary>
    ///     The amount of dirtiness this step should add to tools on completion
    /// </summary>
    [DataField]
    public FixedPoint2 ToolDirtiness = 0.2;

    /// <summary>
    ///     The amount of dirtiness this step should add to gloves on completion
    /// </summary>
    [DataField]
    public FixedPoint2 GloveDirtiness = 0.2;
}
