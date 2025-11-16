// SPDX-FileCopyrightText: 2024 Nemanja <98561806+EmoGarbage404@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tayrtahn <tayrtahn@gmail.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Clock;
using Robust.Client.GameObjects;

namespace Content.Client.Clock;

public sealed class ClockSystem : SharedClockSystem
{
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ClockComponent, SpriteComponent>();
        while (query.MoveNext(out var uid, out var comp, out var sprite))
        {
            if (!_sprite.LayerMapTryGet((uid, sprite), ClockVisualLayers.HourHand, out var hourLayer, false) ||
                !_sprite.LayerMapTryGet((uid, sprite), ClockVisualLayers.MinuteHand, out var minuteLayer, false))
                continue;

            var time = GetClockTime((uid, comp));
            var hourState = $"{comp.HoursBase}{time.Hours % 12}";
            var minuteState = $"{comp.MinutesBase}{time.Minutes / 5}";
            _sprite.LayerSetRsiState((uid, sprite), hourLayer, hourState);
            _sprite.LayerSetRsiState((uid, sprite), minuteLayer, minuteState);
        }
    }
}
