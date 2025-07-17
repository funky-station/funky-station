// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 corresp0nd <46357632+corresp0nd@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.Objectives.Systems;
using Content.Shared._DV.Recruiter;

namespace Content.Server.Objectives.Components;

/// <summary>
/// Objective condition that requires the recruiter's pen to be used by a number of people to sign paper.
/// Requires <see cref="NumberObjectiveComponent"/> to function.
/// </summary>
[RegisterComponent, Access(typeof(RecruitingConditionSystem), typeof(SharedRecruiterPenSystem))]
public sealed partial class RecruitingConditionComponent : Component
{
    [DataField]
    public int Recruited;
}
