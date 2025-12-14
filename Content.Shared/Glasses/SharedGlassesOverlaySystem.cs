// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Glasses;

public sealed class SharedGlassesOverlaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GlassesOverlayComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, GlassesOverlayComponent component, ref ComponentGetState args)
    {
        args.State = new GlassesOverlayComponentState(
            component.Enabled,
            component.Shader,
            component.Color
        );
    }
}
