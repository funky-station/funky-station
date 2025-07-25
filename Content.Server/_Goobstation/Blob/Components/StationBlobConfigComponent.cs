// SPDX-FileCopyrightText: 2024 John Space <bigdumb421@gmail.com>
// SPDX-FileCopyrightText: 2024 fishbait <gnesse@gmail.com>
// SPDX-FileCopyrightText: 2025 Mish <bluscout78@yahoo.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.ComponentModel.DataAnnotations;
//using Content.Server.SpecForces;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Blob.Components;

[RegisterComponent]
public sealed partial class StationBlobConfigComponent : Component
{
    public const int DefaultStageBegin = 80;
    public const int DefaultStageCritical = 400;
    public const int DefaultStageEnd = 800;

    [DataField]
    public int StageBegin { get; set; } = DefaultStageBegin;

    [DataField]
    public int StageCritical { get; set; } = DefaultStageCritical;

    [DataField]
    public int StageTheEnd { get; set; } = DefaultStageEnd;

    /*[DataField("specForceTeam")]  //Goobstation - Disabled automatic ERT
    public ProtoId<SpecForceTeamPrototype> SpecForceTeam { get; set; } = "RXBZZBlobDefault";*/
}
