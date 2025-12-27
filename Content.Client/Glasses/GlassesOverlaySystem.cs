// SPDX-FileCopyrightText: 2025 MaiaArai <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Glasses;
using Content.Shared.Inventory.Events;
using Content.Client.Overlays;
using Robust.Client.Graphics;
using System.Linq;
using Robust.Shared.GameStates;

namespace Content.Client.Glasses;

public sealed class GlassesOverlaySystem : EquipmentHudSystem<GlassesOverlayComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private GlassesOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new();
        SubscribeLocalEvent<GlassesOverlayComponent, ComponentHandleState>(OnHandleState);
    }

    private void OnHandleState(EntityUid uid, GlassesOverlayComponent component, ref ComponentHandleState args)
    {
        if (args.Current is not GlassesOverlayComponentState state)
            return;

        component.Enabled = state.Enabled;
        component.Shader = state.Shader;
        component.Color = state.Color;
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<GlassesOverlayComponent> args)
    {
        base.UpdateInternal(args);

        if (!_overlayMan.HasOverlay<GlassesOverlay>())
            _overlayMan.AddOverlay(_overlay);

        _overlay.Providers = args.Components.Where(c => c.Enabled).ToHashSet();
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();
        _overlayMan.RemoveOverlay(_overlay);
    }
}
