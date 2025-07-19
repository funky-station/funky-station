// SPDX-FileCopyrightText: 2024 PJBot <pieterjan.briers+bot@gmail.com>
// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 username <113782077+whateverusername0@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 whateverusername0 <whateveremail>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.GameTicking.Rules;
using Content.Shared.Dataset;
using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Prototypes;

namespace Content.Server.StationEvents;

[RegisterComponent]
public sealed partial class DynamicStationEventSchedulerComponent : Component
{
    [DataField] public ProtoId<DatasetPrototype> MidroundRulesPool;
    [ViewVariables(VVAccess.ReadOnly)] public List<EntProtoId> ExecutedRules = new();

    /// <summary>
    ///     How long until the next check for an event runs, is initially set based on MinimumTimeUntilFirstEvent & MinMaxEventTiming.
    /// </summary>
    [DataField] public float EventClock = 1200f;

    [DataField] public float? Budget = null;

    /// <summary>
    ///     How much time it takes in seconds for an antag event to be raised.
    /// </summary>
    [DataField] public MinMax Delays = new(15 * 60, 20 * 60);
}
