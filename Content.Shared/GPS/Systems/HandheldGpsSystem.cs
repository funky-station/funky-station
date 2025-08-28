// SPDX-FileCopyrightText: 2025 Tay <td12233a@gmail.com>
// SPDX-FileCopyrightText: 2025 pa.pecherskij <pa.pecherskij@interfax.ru>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.GPS.Components;
using Content.Shared.Examine;
using Robust.Shared.Map;

namespace Content.Shared.GPS.Systems;

public sealed class HandheldGpsSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandheldGPSComponent, ExaminedEvent>(OnExamine);
    }

    /// <summary>
    /// Handles showing the coordinates when a GPS is examined.
    /// </summary>
    private void OnExamine(Entity<HandheldGPSComponent> ent, ref ExaminedEvent args)
    {
        var posText = "Error";

        var pos = _transform.GetMapCoordinates(ent);

        if (pos.MapId != MapId.Nullspace)
        {
            var x = (int) pos.Position.X;
            var y = (int) pos.Position.Y;
            posText = $"({x}, {y})";
        }

        args.PushMarkup(Loc.GetString("handheld-gps-coordinates-title", ("coordinates", posText)));
    }
}
