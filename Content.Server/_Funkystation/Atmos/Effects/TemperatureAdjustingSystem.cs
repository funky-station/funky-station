// SPDX-FileCopyrightText: 2023 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 ThunderBear2006 <100388962+ThunderBear2006@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Steve <marlumpy@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Server.Atmos.EntitySystems;
using Content.Server.Anomaly.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Funkystation.Atmos.Effects;

/// <summary>
/// This handles <see cref="TemperatureAdjustingComponent"/>
/// </summary>
public sealed class TemperatureAdjustingSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly TransformSystem _xform = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TemperatureAdjustingComponent, TransformComponent>();
        while (query.MoveNext(out var ent, out var comp, out var xform))
        {
            var grid = xform.GridUid;
            var map = xform.MapUid;
            var indices = _xform.GetGridTilePositionOrDefault((ent, xform));
            var mixture = _atmosphere.GetTileMixture(grid, map, indices, true);

            if (mixture is { })
            {
                var tempChange = comp.TempChangePerSecond * frameTime;
                if (tempChange > 0 && mixture.Temperature + tempChange > comp.MaxTemperature)
                {
                    tempChange = comp.MaxTemperature - mixture.Temperature;
                }
                else if (tempChange < 0 && mixture.Temperature + tempChange < comp.MinTemperature)
                {
                    tempChange = comp.MinTemperature - mixture.Temperature;
                }
                mixture.Temperature += tempChange;
            }
        }
    }
}
