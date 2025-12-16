// SPDX-FileCopyrightText: 2025 MaiaArai <158123176+YaraaraY@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Clothing.Components;

namespace Content.Shared.Clothing.EntitySystems;

public sealed class MultiVisualStateSystem : EntitySystem
{
    [Dependency] private readonly ClothingSystem _clothingSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MultiVisualStateComponent, AfterAutoHandleStateEvent>(OnStateChanged);
        SubscribeLocalEvent<MultiVisualStateComponent, ItemHeadToggledEvent>(OnHeadToggled);
    }

    private void OnHeadToggled(Entity<MultiVisualStateComponent> ent, ref ItemHeadToggledEvent args)
    {
        // For hardsuit: IsToggled=true means Visor is DOWN (Active)
        ent.Comp.VisorState = args.IsToggled;
        UpdateVisuals(ent);
    }

    private void OnStateChanged(Entity<MultiVisualStateComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        UpdateVisuals(ent);
    }

    public void UpdateVisuals(Entity<MultiVisualStateComponent> ent)
    {
        var (uid, comp) = ent;
        string? finalPrefix = null;

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
