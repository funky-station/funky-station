// SPDX-FileCopyrightText: 2026 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Footprint;
using Robust.Client.GameObjects;
using Robust.Shared.Utility;

namespace Content.Client.Footprint;

public sealed class FootprintSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<FootprintComponent, ComponentStartup>(OnComponentStartup);
        SubscribeNetworkEvent<FootprintChangedEvent>(OnFootprintChanged);
    }

    private void OnComponentStartup(Entity<FootprintComponent> entity, ref ComponentStartup e)
    {
        UpdateSprite(entity, entity);
    }

    private void OnFootprintChanged(FootprintChangedEvent e)
    {
        if (!TryGetEntity(e.Entity, out var entity))
            return;

        if (!TryComp<FootprintComponent>(entity, out var footprint))
            return;

        UpdateSprite(entity.Value, footprint);
    }

    private void UpdateSprite(EntityUid entity, FootprintComponent footprint)
    {
        if (!TryComp<SpriteComponent>(entity, out var sprite))
            return;

        // Note: You need a generic footprint RSI here with states "foot" and "body"
        var rsiPath = new ResPath("/Textures/Effects/footprints.rsi");

        for (var i = 0; i < footprint.Footprints.Count; i++)
        {
            if (!sprite.LayerExists(i))
                sprite.AddBlankLayer(i);

            // Set the transform and color
            sprite.LayerSetOffset(i, footprint.Footprints[i].Offset);
            sprite.LayerSetRotation(i, footprint.Footprints[i].Rotation);
            sprite.LayerSetColor(i, footprint.Footprints[i].Color);

            // Set the sprite state (generic foot or body drag)
            sprite.LayerSetSprite(i, new SpriteSpecifier.Rsi(rsiPath, footprint.Footprints[i].State));
        }
    }
}
