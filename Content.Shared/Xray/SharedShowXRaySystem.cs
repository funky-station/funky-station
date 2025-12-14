// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.XRay;

public sealed class SharedShowXRaySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ShowXRayComponent, ComponentGetState>(OnGetState);
    }

    private void OnGetState(EntityUid uid, ShowXRayComponent component, ref ComponentGetState args)
    {
        args.State = new ShowXRayComponentState(
            component.Enabled,
            component.Shader,
            component.EntityRange,
            component.TileRange
        );
    }
}
