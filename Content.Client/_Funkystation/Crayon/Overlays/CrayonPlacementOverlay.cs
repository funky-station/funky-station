// SPDX-FileCopyrightText: 2025 Ekpy <33184056+Ekpy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later AND MIT

using System.Numerics;
using Content.Client.Crayon;
using Content.Client.Hands.Systems;
using Content.Shared.CCVar;
using Content.Shared.Decals;
using Content.Shared.Humanoid;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Crayon.Overlays;

public sealed class CrayonPlacementOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IEntityManager _entMan = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private HandsSystem? _hands;
    private EntityUid? _lastActiveHandUid;
    private CrayonComponent? _lastActiveCrayonComp;

    private readonly SpriteSystem _sprite;
    private readonly SharedTransformSystem _transform;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowFOV;

    public CrayonPlacementOverlay(SharedTransformSystem transform, SpriteSystem spriteSystem)
    {
        IoCManager.InjectDependencies(this);
        _transform = transform;
        _sprite = spriteSystem;
        ZIndex = 1000;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var transparency = _cfg.GetCVar(CCVars.CrayonOverlayTransparency);
        // If transparency is 0, don't draw the overlay at all.
        if (transparency <= 0f)
            return;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        // Offscreen
        if (mouseScreenPos.Window == WindowId.Invalid)
            return;

        var mousePos = _eyeManager.PixelToMap(mouseScreenPos);
        // Not on grid. Can't draw on non grid
        if (!_mapManager.TryFindGridAt(mousePos, out var gridUid, out var grid))
            return;

        _hands ??= _entMan.System<HandsSystem>();
        var handEntity = _hands.GetActiveHandEntity();

        if (handEntity is null)
            return;

        // Making it so that the overlay only tries to get component when the active hand changes
        if (handEntity != _lastActiveHandUid)
        {
            _lastActiveHandUid = handEntity;
            _lastActiveCrayonComp = _entMan.TryGetComponent(handEntity, out CrayonComponent? crayon) ? crayon : null;
        }

        if (_lastActiveCrayonComp == null)
            return;

        if (!_prototypeManager.TryIndex<DecalPrototype>(_lastActiveCrayonComp.SelectedState, out var decal))
            return;

        // Copying code from DecalPlacementOverlay, so the overlay matches the placement logic of decals.
        var worldMatrix = _transform.GetWorldMatrix(gridUid);
        var invMatrix = _transform.GetInvWorldMatrix(gridUid);

        var handle = args.WorldHandle;
        handle.SetTransform(worldMatrix);

        var localPos = Vector2.Transform(mousePos.Position, invMatrix);

        var aabb = Box2.UnitCentered.Translated(localPos);
        var box = new Box2Rotated(aabb, Angle.Zero, localPos);

        handle.DrawTextureRect(_sprite.Frame0(decal.Sprite), box, _lastActiveCrayonComp.Color * Color.White.WithAlpha(transparency));
        handle.SetTransform(Matrix3x2.Identity);
    }
}
