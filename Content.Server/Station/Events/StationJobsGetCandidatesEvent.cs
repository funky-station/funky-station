// SPDX-FileCopyrightText: 2024 DrSmugleaf <10968691+DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server.Station.Events;

[ByRefEvent]
public readonly record struct StationJobsGetCandidatesEvent(NetUserId Player, List<ProtoId<JobPrototype>> Jobs);
