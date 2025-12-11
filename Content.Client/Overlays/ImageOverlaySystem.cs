// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing;
using Content.Shared.Item.ItemToggle.Components;

namespace Content.Client.Overlays;

public sealed class ImageOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly InventorySystem _inventorySystem = default!;

    public static readonly ProtoId<ShaderPrototype> ImageShader = "ImageMask";
    private ImageOverlay _overlay = new();

    public override void Initialize()
    {
        base.Initialize();
        _overlay = new();

        SubscribeLocalEvent<ImageOverlayComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ImageOverlayComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<ImageOverlayComponent, AfterAutoHandleStateEvent>(OnHandleState);
        SubscribeLocalEvent<ImageOverlayComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ImageOverlayComponent, GotUnequippedEvent>(OnUnequipped);
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);
        SubscribeLocalEvent<ImageOverlayComponent, ItemMaskToggledEvent>(OnMaskToggled);
        SubscribeLocalEvent<ImageOverlayComponent, ItemToggledEvent>(OnItemToggled);
    }

    private void OnStartup(EntityUid uid, ImageOverlayComponent comp, ComponentStartup args)
    {
        RefreshOverlay();
    }

    private void OnShutdown(EntityUid uid, ImageOverlayComponent comp, ComponentShutdown args)
    {
        RefreshOverlay(ignoreComp: comp);
    }

    private void OnHandleState(EntityUid uid, ImageOverlayComponent comp, ref AfterAutoHandleStateEvent args)
    {
        RefreshOverlay();
    }

    private void OnEquipped(EntityUid uid, ImageOverlayComponent comp, GotEquippedEvent args)
    {
        RefreshOverlay();
    }

    private void OnUnequipped(EntityUid uid, ImageOverlayComponent comp, GotUnequippedEvent args)
    {
        RefreshOverlay();
    }

    private void OnPlayerAttached(PlayerAttachedEvent args)
    {
        RefreshOverlay();
    }

    private void OnPlayerDetached(PlayerDetachedEvent args)
    {
        _overlay.ImageShaders.Clear();
        _overlayMan.RemoveOverlay(_overlay);
    }

    private void OnMaskToggled(EntityUid uid, ImageOverlayComponent comp, ref ItemMaskToggledEvent args)
    {
        RefreshOverlay(forcedToggleEntity: uid, forcedToggleState: args.IsToggled);
    }

    private void OnItemToggled(EntityUid uid, ImageOverlayComponent comp, ref ItemToggledEvent args)
    {
        RefreshOverlay();
    }

    private void RefreshOverlay(ImageOverlayComponent? ignoreComp = null, EntityUid? forcedToggleEntity = null, bool? forcedToggleState = null)
    {
        var player = _playerManager.LocalSession?.AttachedEntity;

        if (player == null)
            return;

        _overlay.ImageShaders.Clear();
        bool hasOverlay = false;

        var slotsToCheck = new[] { "head", "eyes", "mask" };

        foreach (var slot in slotsToCheck)
        {
            if (_inventorySystem.TryGetSlotEntity(player.Value, slot, out var item))
            {
                if (TryComp<ImageOverlayComponent>(item, out var comp))
                {
                    if (comp == ignoreComp)
                        continue;

                    bool shouldApplyOverlay = true;

                    if (forcedToggleEntity != null && item == forcedToggleEntity.Value && forcedToggleState.HasValue)
                    {
                        if (forcedToggleState.Value)
                            shouldApplyOverlay = false;
                    }
                    else if (TryComp<ItemToggleComponent>(item, out var toggle))
                    {
                        if (!toggle.Activated)
                            shouldApplyOverlay = false;
                    }
                    else if (TryComp<MaskComponent>(item, out var mask))
                    {
                        if (mask.IsToggled)
                            shouldApplyOverlay = false;
                    }

                    if (!shouldApplyOverlay)
                        continue;

                    if (comp.PathToOverlayImage == null)
                        continue;

                    var values = new ImageShaderValues
                    {
                        PathToOverlayImage = comp.PathToOverlayImage.Value,
                        AdditionalColorOverlay = comp.AdditionalColorOverlay
                    };

                    _overlay.ImageShaders.Add((_prototypeManager.Index(ImageShader).InstanceUnique(), values));
                    hasOverlay = true;
                }
            }
        }

        if (hasOverlay)
        {
            if (!_overlayMan.HasOverlay<ImageOverlay>())
                _overlayMan.AddOverlay(_overlay);
        }
        else
        {
            _overlayMan.RemoveOverlay(_overlay);
        }
    }
}
