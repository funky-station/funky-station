// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 slarticodefast <161409025+slarticodefast@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using System.Numerics;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Client.Movement.Systems;

public sealed class ContentEyeSystem : SharedContentEyeSystem
{
    [Dependency] private readonly IPlayerManager _player = default!;

    public void RequestZoom(EntityUid uid, Vector2 zoom, bool ignoreLimit, bool scalePvs, ContentEyeComponent? content = null)
    {
        if (!Resolve(uid, ref content, false))
            return;

        RaisePredictiveEvent(new RequestTargetZoomEvent()
        {
            TargetZoom = zoom,
            IgnoreLimit = ignoreLimit,
        });

        if (scalePvs)
            RequestPvsScale(Math.Max(zoom.X, zoom.Y));
    }

    public void RequestPvsScale(float scale)
    {
        RaiseNetworkEvent(new RequestPvsScaleEvent(scale));
    }

    public void RequestToggleFov()
    {
        if (_player.LocalEntity is { } player)
            RequestToggleFov(player);
    }

    public void RequestToggleFov(EntityUid uid, EyeComponent? eye = null)
    {
        if (Resolve(uid, ref eye, false))
            RequestEye(!eye.DrawFov, eye.DrawLight);
    }

    public void RequestToggleLight(EntityUid uid, EyeComponent? eye = null)
    {
        if (Resolve(uid, ref eye, false))
            RequestEye(eye.DrawFov, !eye.DrawLight);
    }


    public void RequestEye(bool drawFov, bool drawLight)
    {
        RaisePredictiveEvent(new RequestEyeEvent(drawFov, drawLight));
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);
        var eyeEntities = AllEntityQuery<ContentEyeComponent, EyeComponent>();
        while (eyeEntities.MoveNext(out var entity, out ContentEyeComponent? contentComponent, out EyeComponent? eyeComponent))
        {
            UpdateEyeOffset((entity, eyeComponent));
        }
    }
}
