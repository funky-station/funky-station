// SPDX-FileCopyrightText: 2025 ALooseGoose <ALooseGoosey@gmail.com>
// SPDX-FileCopyrightText: 2025 TheSecondLord <88201625+TheSecondLord@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 V <97265903+formlessnameless@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared._EE.Overlays.Switchable;
using Robust.Client.Graphics;
using Robust.Client.GameObjects;
using Robust.Client.Player;

namespace Content.Client._EE.Overlays.Switchable;

public sealed class NightVisionSystem : Client.Overlays.EquipmentHudSystem<NightVisionComponent>
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly ILightManager _lightManager = default!;
    [Dependency] private readonly PointLightSystem _pointLightSystem = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly TransformSystem _transformSystem = default!;
    private BaseSwitchableOverlay<NightVisionComponent> _overlay = default!;
    private EntityUid? _nvLight;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NightVisionComponent, SwitchableOverlayToggledEvent>(OnToggle);

        _overlay = new BaseSwitchableOverlay<NightVisionComponent>();
    }

    protected override void OnRefreshComponentHud(Entity<NightVisionComponent> ent, ref RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.OnRefreshComponentHud(ent, ref args);
    }

    protected override void OnRefreshEquipmentHud(Entity<NightVisionComponent> ent, ref InventoryRelayedEvent<RefreshEquipmentHudEvent<NightVisionComponent>> args)
    {
        if (!ent.Comp.IsEquipment)
            return;

        base.OnRefreshEquipmentHud(ent, ref args);
    }

    private void OnToggle(Entity<NightVisionComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        RefreshOverlay();
    }

    protected override void UpdateInternal(RefreshEquipmentHudEvent<NightVisionComponent> args)
    {
        base.UpdateInternal(args);

        var active = false;
        NightVisionComponent? nvComp = null;
        foreach (var comp in args.Components)
        {
            if (comp.IsActive || comp.PulseTime > 0f && comp.PulseAccumulator < comp.PulseTime)
                active = true;
            else
                continue;

            if (comp.DrawOverlay)
            {
                if (nvComp == null)
                    nvComp = comp;
                else if (nvComp.PulseTime > 0f && comp.PulseTime <= 0f)
                    nvComp = comp;
            }

            if (active && nvComp is { PulseTime: <= 0 })
                break;
        }

        if (_playerManager.LocalEntity is { } player)
        {
            if (active)
                EnableNightVision(player);
            else
                DisableNightVision();
        }

        UpdateOverlay(nvComp);
    }

    protected override void DeactivateInternal()
    {
        base.DeactivateInternal();

        DisableNightVision();
        UpdateOverlay(null);
    }

    private void UpdateNightVision(bool active, EntityUid player)
    {
        _lightManager.DrawShadows = !active;
    }

    private void UpdateOverlay(NightVisionComponent? nvComp)
    {
        _overlay.Comp = nvComp;

        switch (nvComp)
        {
            case not null when !_overlayMan.HasOverlay<BaseSwitchableOverlay<NightVisionComponent>>():
                _overlayMan.AddOverlay(_overlay);
                break;
            case null:
                _overlayMan.RemoveOverlay(_overlay);
                break;
        }

        if (_overlayMan.TryGetOverlay<BaseSwitchableOverlay<ThermalVisionComponent>>(out var overlay))
            overlay.IsActive = nvComp == null;
    }

    private void EnableNightVision(EntityUid player)
    {
        if (_nvLight != null && _entMan.EntityExists(_nvLight.Value))
            return;

        var ent = _entMan.SpawnEntity(null, Transform(player).Coordinates);
        _nvLight = ent;
        _transformSystem.SetParent(ent, player);
        var light = _entMan.AddComponent<PointLightComponent>(ent);
        _pointLightSystem.SetRadius(ent, 999f, light);
    }
    private void DisableNightVision()
    {
        if (_nvLight is { } ent && _entMan.EntityExists(ent))
            _entMan.DeleteEntity(ent);

        _nvLight = null;
    }
}
