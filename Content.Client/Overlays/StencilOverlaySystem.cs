// SPDX-FileCopyrightText: 2024 Tadeo <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2024 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Parallax;
using Content.Client.Weather;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;

namespace Content.Client.Overlays;

public sealed class StencilOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly ParallaxSystem _parallax = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly WeatherSystem _weather = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new StencilOverlay(_parallax, _transform, _map, _sprite, _weather));
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlay.RemoveOverlay<StencilOverlay>();
    }
}
