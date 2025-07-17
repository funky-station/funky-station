// SPDX-FileCopyrightText: 2025 misghast <51974455+misterghast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using Content.Server._Goobstation.StationEvents.Metric.Components;
using Content.Server.Spreader;
using Content.Shared.Anomaly.Components;
using Content.Shared.FixedPoint;

namespace Content.Server._Goobstation.StationEvents.Metric;

/// <summary>
///   Measures the number and severity of anomalies on the station.
///
///   Writes this to the Anomaly chaos value.
/// </summary>
public sealed class AnomalyMetric : ChaosMetricSystem<AnomalyMetricComponent>
{
    public override ChaosMetrics CalculateChaos(EntityUid metricUid,
        AnomalyMetricComponent component,
        CalculateChaosEvent args)
    {
        var anomalyChaos = FixedPoint2.Zero;

        // Consider each anomaly and add its stability and growth to the accumulator
        var anomalyQ = EntityQueryEnumerator<AnomalyComponent>();
        while (anomalyQ.MoveNext(out var uid, out var anomaly))
        {
            if (anomaly.Severity > 0.8f)
            {
                anomalyChaos += component.SeverityCost;
            }

            if (anomaly.Stability > anomaly.GrowthThreshold)
            {
                anomalyChaos += component.GrowingCost;
            }

            anomalyChaos += component.BaseCost;
        }

        var kudzuQ = EntityQueryEnumerator<KudzuComponent>();
        while (kudzuQ.MoveNext(out var uid, out var kudzu))
        {
            anomalyChaos += 0.25f;
        }

    var chaos = new ChaosMetrics(new Dictionary<ChaosMetric, FixedPoint2>()
        {
            {ChaosMetric.Anomaly, anomalyChaos},
        });
        return chaos;
    }
}
