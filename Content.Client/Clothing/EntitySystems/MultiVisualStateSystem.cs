// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects;

namespace Content.Client.Clothing.EntitySystems;

public sealed class MultiVisualClientSystem : EntitySystem
{
    // dependency on the shared logic to avoid code duplication
    [Dependency] private readonly MultiVisualStateSystem _sharedSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        // This is the Client-side only event
        SubscribeLocalEvent<MultiVisualStateComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnAppearanceChanged(Entity<MultiVisualStateComponent> ent, ref AppearanceChangeEvent args)
    {
        // Update the local component state based on Appearance data
        if (_appearance.TryGetData(ent.Owner, ToggleableLightVisuals.Enabled, out bool lightEnabled, args.Component))
        {
            ent.Comp.LightState = lightEnabled;

            // Re-run the logic to determine the correct sprite prefix
            _sharedSystem.UpdateVisuals(ent);
        }
    }
}
