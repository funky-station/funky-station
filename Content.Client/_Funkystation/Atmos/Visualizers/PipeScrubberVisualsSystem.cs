// SPDX-FileCopyrightText: 2026 Steve <marlumpy@gmail.com>
//
// SPDX-License-Identifier: MIT

using Robust.Client.GameObjects;
using Content.Shared._Funkystation.Atmos.Visuals;

namespace Content.Client._Funkystation.Atmos.Visualizers;

public sealed class PipeScrubberVisualizerSystem : VisualizerSystem<PipeScrubberVisualsComponent>
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    protected override void OnAppearanceChange(EntityUid uid, PipeScrubberVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        bool isEnabled;
        bool isFull;
        bool isScrubbing;
        bool isDraining;

        AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsEnabled, out isEnabled, args.Component);
        AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsFull, out isFull, args.Component);
        AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsScrubbing, out isScrubbing, args.Component);
        AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsDraining, out isDraining, args.Component);

        bool visibleLayer = true;

        if (isDraining)
            _sprite.LayerSetRsiState((uid, args.Sprite), 1, component.DrainingState);
        else if (isFull)
            _sprite.LayerSetRsiState((uid, args.Sprite), 1, component.FullState);
        else if (isScrubbing)
            _sprite.LayerSetRsiState((uid, args.Sprite), 1, component.ScrubbingState);
        else if (isEnabled)
            _sprite.LayerSetRsiState((uid, args.Sprite), 1, component.EnabledState);
        else
        {
            _sprite.LayerSetRsiState((uid, args.Sprite), 1, component.IdleState);
            visibleLayer = false;
        }

        _sprite.LayerSetVisible((uid, args.Sprite), 1, visibleLayer);

        if (AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsFull, out var fullData, args.Component))
        {
            _sprite.LayerSetVisible((uid, args.Sprite), 2, fullData && !isDraining && !isScrubbing);
            if (fullData && !isDraining && !isScrubbing)
                _sprite.LayerSetRsiState((uid, args.Sprite), 2, component.FullState);
        }

        if (AppearanceSystem.TryGetData<bool>(uid, PipeScrubberVisuals.IsDraining, out var drainingData, args.Component))
        {
            _sprite.LayerSetVisible((uid, args.Sprite), 3, drainingData);
            if (drainingData)
                _sprite.LayerSetRsiState((uid, args.Sprite), 3, component.DrainingState);
        }
    }
}
