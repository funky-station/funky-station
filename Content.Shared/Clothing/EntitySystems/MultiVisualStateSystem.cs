// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.Components;
using Content.Shared.Toggleable;
using Robust.Client.GameObjects; // Required for ToggleableLightVisuals

namespace Content.Shared.Clothing.EntitySystems;

public sealed class MultiVisualStateSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultiVisualStateComponent, AfterAutoHandleStateEvent>(OnStateChanged);

        // 1. Listen for VISOR toggles (Custom Event)
        SubscribeLocalEvent<MultiVisualStateComponent, ItemHeadToggledEvent>(OnHeadToggled);

        // 2. Listen for LIGHT toggles via Appearance (Avoids event conflict)
        SubscribeLocalEvent<MultiVisualStateComponent, AppearanceChangeEvent>(OnAppearanceChanged);
    }

    private void OnHeadToggled(Entity<MultiVisualStateComponent> ent, ref ItemHeadToggledEvent args)
    {
        // For hardsuit: IsToggled=true means Visor is DOWN (Active)
        ent.Comp.VisorState = args.IsToggled;
        UpdateVisuals(ent);
    }

    private void OnAppearanceChanged(Entity<MultiVisualStateComponent> ent, ref AppearanceChangeEvent args)
    {
        // Check if the appearance update contains light data.
        // ToggleableLightVisuals.Enabled is the standard key for hand-held lights/hardsuit lights.
        if (_appearance.TryGetData(ent.Owner, ToggleableLightVisuals.Enabled, out bool lightEnabled, args.Component))
        {
            ent.Comp.LightState = lightEnabled;
            UpdateVisuals(ent);
        }
    }

    private void OnStateChanged(Entity<MultiVisualStateComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(ent);
    }

    private void UpdateVisuals(Entity<MultiVisualStateComponent> ent)
    {
        var (uid, comp) = ent;
        string? finalPrefix = null;

        // Select prefix based on the combination of states
        if (comp.VisorState && comp.LightState)
            finalPrefix = comp.PrefixVisorOnLightOn;
        else if (comp.VisorState && !comp.LightState)
            finalPrefix = comp.PrefixVisorOnLightOff;
        else if (!comp.VisorState && comp.LightState)
            finalPrefix = comp.PrefixVisorOffLightOn;
        else
            finalPrefix = comp.PrefixVisorOffLightOff;

        _clothingSystem.SetEquippedPrefix(uid, finalPrefix);
        Dirty(uid, comp);
    }
}
