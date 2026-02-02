// SPDX-FileCopyrightText: 2026 Terkala <appleorange64@gmail.com>
//
// SPDX-License-Identifier: MIT

using Content.Shared.Atmos;
using Content.Shared.Zombies;
using Robust.Client.GameObjects;

namespace Content.Client.Zombies;

/// <summary>
/// Visualizer system that changes the base sprite state of zombie tumors when they're on fire or dead.
/// Unlike FireVisuals which adds an overlay, this changes the base sprite state.
/// </summary>
public sealed class ZombieTumorBurningVisualizerSystem : VisualizerSystem<ZombieTumorBurningVisualsComponent>
{
    protected override void OnAppearanceChange(EntityUid uid, ZombieTumorBurningVisualsComponent component, ref AppearanceChangeEvent args)
    {
        if (args.Sprite == null)
            return;

        // Check if dead state (set by server when destructible threshold reached)
        if (AppearanceSystem.TryGetData<bool>(uid, ZombieTumorBurningVisuals.Dead, out var isDead, args.Component) && isDead)
        {
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, component.DeadState);
            return;
        }

        // Check if on fire (set by FlammableSystem)
        if (AppearanceSystem.TryGetData<bool>(uid, FireVisuals.OnFire, out var onFire, args.Component) && onFire)
        {
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, component.BurningState);
        }
        else
        {
            // Not on fire and not dead - use normal state
            SpriteSystem.LayerSetRsiState((uid, args.Sprite), 0, component.NormalState);
        }
    }
}
