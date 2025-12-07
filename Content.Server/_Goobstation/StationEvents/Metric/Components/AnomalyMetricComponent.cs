// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server.StationEvents.Events;
using Content.Shared.FixedPoint;

namespace Content.Server._Goobstation.StationEvents.Metric.Components;

[RegisterComponent, Access(typeof(AnomalyMetric))]
public sealed partial class AnomalyMetricComponent : Component
{
    /// <summary>
    ///   Cost of a growing anomaly
    /// </summary>
    [DataField("growingCost")]
    public float GrowingCost = 40.0f;

    /// <summary>
    ///   Cost of a dangerous anomaly
    /// </summary>
    [DataField("severityCost")]
    public float SeverityCost = 20.0f;

    /// <summary>
    ///   Cost of any anomaly
    /// </summary>
    [DataField("dangerCost")]
    public float BaseCost = 10.0f;
}
