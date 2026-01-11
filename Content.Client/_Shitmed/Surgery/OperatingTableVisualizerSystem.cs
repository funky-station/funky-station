// SPDX-FileCopyrightText: 2025 otokonoko-dev <248204705+otokonoko-dev@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Surgery;
using Robust.Client.GameObjects;

namespace Content.Client._Shitmed.Surgery;

public sealed class OperatingTableVisualizerSystem : VisualizerSystem<OperatingTableVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, OperatingTableVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite is not { } sprite)
            return;

        if (AppearanceSystem.TryGetData<bool>(uid, OperatingTableVisuals.LightOn, out var lightOn, args.Component))
        {
            if (sprite.LayerMapTryGet(OperatingTableVisualLayers.Base, out var baseLayer))
            {
                var state = lightOn ? "operating_table_on" : "operating_table";
                sprite.LayerSetState(baseLayer, state);
            }
        }

        if (AppearanceSystem.TryGetData<VitalsState>(uid, OperatingTableVisuals.VitalsState, out var vitalsState, args.Component))
        {
            if (sprite.LayerMapTryGet(OperatingTableVisualLayers.Vitals, out var vitalsLayer))
            {
                switch (vitalsState)
                {
                    case VitalsState.None:
                        sprite.LayerSetVisible(vitalsLayer, false);
                        break;
                    case VitalsState.Healthy:
                        sprite.LayerSetState(vitalsLayer, "vitals_1");
                        sprite.LayerSetVisible(vitalsLayer, true);
                        break;
                    case VitalsState.Injured:
                        sprite.LayerSetState(vitalsLayer, "vitals_2");
                        sprite.LayerSetVisible(vitalsLayer, true);
                        break;
                    case VitalsState.Critical:
                        sprite.LayerSetState(vitalsLayer, "vitals_3");
                        sprite.LayerSetVisible(vitalsLayer, true);
                        break;
                    case VitalsState.Dead:
                        sprite.LayerSetState(vitalsLayer, "vitals_4");
                        sprite.LayerSetVisible(vitalsLayer, true);
                        break;
                }
            }
        }
    }
}
