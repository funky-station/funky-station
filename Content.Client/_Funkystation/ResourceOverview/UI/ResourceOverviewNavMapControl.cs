// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Client.Pinpointer.UI;
using Content.Shared._Funkystation.ResourceOverview.BUI;
using Robust.Client.Graphics;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Client._Funkystation.ResourceOverview.UI;

public sealed class ResourceOverviewNavMapControl : NavMapControl
{
    public List<ResourceOverviewConnection> Connections { get; } = new();

    public ResourceOverviewNavMapControl()
    {
        PostWallDrawingAction += DrawLatheSiloLines;
    }

    private void DrawLatheSiloLines(DrawingHandleScreen handle)
    {
        if (Connections.Count == 0)
            return;

        if (MapUid == null || !EntManager.TryGetComponent<TransformComponent>(MapUid, out var xform))
            return;

        var transformSystem = EntManager.System<SharedTransformSystem>();
        var offset = GetOffset();

        var lines = new ValueList<Vector2>();

        foreach (var connection in Connections)
        {
            var latheEntity = EntManager.GetEntity(connection.LatheNetEntity);
            var siloEntity = EntManager.GetEntity(connection.SiloNetEntity);

            if (!EntManager.TryGetComponent<TransformComponent>(latheEntity, out var latheXform) ||
                !EntManager.TryGetComponent<TransformComponent>(siloEntity, out var siloXform))
                continue;

            var latheMapPos = transformSystem.ToMapCoordinates(latheXform.Coordinates);
            var siloMapPos = transformSystem.ToMapCoordinates(siloXform.Coordinates);

            if (latheMapPos.MapId == MapId.Nullspace || siloMapPos.MapId == MapId.Nullspace)
                continue;

            var lathePosition = Vector2.Transform(latheMapPos.Position, transformSystem.GetInvWorldMatrix(xform)) - offset;
            var siloPosition = Vector2.Transform(siloMapPos.Position, transformSystem.GetInvWorldMatrix(xform)) - offset;

            var start = ScalePosition(new Vector2(lathePosition.X, -lathePosition.Y));
            var end = ScalePosition(new Vector2(siloPosition.X, -siloPosition.Y));

            lines.Add(start);
            lines.Add(end);
        }

        if (lines.Count > 0)
        {
            var color = Color.ToSrgb(new Color(100, 149, 237));
            handle.DrawPrimitives(DrawPrimitiveTopology.LineList, lines.Span, color);
        }
    }
}
