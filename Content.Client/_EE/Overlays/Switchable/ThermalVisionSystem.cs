// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client._EE.Overlays.Switchable;
using Content.Client.Overlays;
using Content.Goobstation.Shared.Overlays;
using Content.Shared._EE.Overlays.Switchable;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Client.Graphics;

namespace Content.Goobstation.Client.Overlays;

public sealed class ThermalVisionSystem : EquipmentHudSystem<Shared.Overlays.ThermalVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private ThermalVisionOverlay _thermalOverlay = default!;
    private BaseSwitchableOverlay<Shared.Overlays.ThermalVisionComponent> _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<Shared.Overlays.ThermalVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _thermalOverlay = new ThermalVisionOverlay();
        _overlay = new BaseSwitchableOverlay<Shared.Overlays.ThermalVisionComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<Shared.Overlays.ThermalVisionComponent> ent,
        ref RefreshEquipmentHudEvent<Shared.Overlays.ThermalVisionComponent> args)
    {
        if (!ent.Comp.IsEquipment)
            base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<Shared.Overlays.ThermalVisionComponent> ent,
        ref InventoryRelayedEvent<RefreshEquipmentHudEvent<Shared.Overlays.ThermalVisionComponent>> args)
    {
        if (ent.Comp.IsEquipment)
            base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<Shared.Overlays.ThermalVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<Shared.Overlays.ThermalVisionComponent> args)
    {
        base.UpdateInternal(args);
        Shared.Overlays.ThermalVisionComponent? tvComp = null;
        var lightRadius = 0f;
        foreach (var comp in args.Components)
        {
            if (!comp.IsActive && (comp.PulseTime <= 0f || comp.PulseAccumulator >= comp.PulseTime))
                continue;

            if (tvComp == null)
                tvComp = comp;
            else if (!tvComp.DrawOverlay && comp.DrawOverlay)
                tvComp = comp;
            else if (tvComp.DrawOverlay == comp.DrawOverlay && tvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                tvComp = comp;

            lightRadius = MathF.Max(lightRadius, comp.LightRadius);
        }

        UpdateThermalOverlay(tvComp, lightRadius);
        UpdateOverlay(tvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        _thermalOverlay.ResetLight(false);
        UpdateOverlay(null);
        UpdateThermalOverlay(null, 0f);
    }

    private void UpdateThermalOverlay(Shared.Overlays.ThermalVisionComponent? comp, float lightRadius)
    {
        _thermalOverlay.LightRadius = lightRadius;
        _thermalOverlay.Comp = comp;

        switch (comp)
        {
            case not null when !_overlayMan.HasOverlay<ThermalVisionOverlay>():
                _overlayMan.AddOverlay(_thermalOverlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_thermalOverlay);
                _thermalOverlay.ResetLight();
                break;
        }
    }

    private void UpdateOverlay(Shared.Overlays.ThermalVisionComponent? tvComp)
    {
        _overlay.Comp = tvComp;

        switch (tvComp)
        {
            case { DrawOverlay: true } when !_overlayMan.HasOverlay<BaseSwitchableOverlay<Shared.Overlays.ThermalVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null or { DrawOverlay: false }:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        // Night vision overlay is prioritized
        _overlay.IsActive = !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>();
    }
}
