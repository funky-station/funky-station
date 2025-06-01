using System.Numerics;
using Content.Client.Crayon;
using Content.Shared.Decals;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Client.Decals.Overlays;

public sealed class CrayonPlacementOverlay : Overlay
{
    [Dependency] private readonly IEyeManager _eyeManager = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    //private readonly CrayonSystem _placement;

    [Dependency] private readonly SpriteSystem _sprite = default!;
    private readonly SharedTransformSystem _transform;

    private DecalPrototype? _decal;
    private Color _color;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    public CrayonPlacementOverlay(SharedTransformSystem transform)
    {
        IoCManager.InjectDependencies(this);
        _transform = transform;
        ZIndex = 1000;
    }

    public void SetActiveDecal(DecalPrototype decal, Color color)
    {
        _decal = decal;
        _color = color;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        //var (decal, snap, rotation, color) = _placement.GetActiveDecal();

        if (_decal == null)
            return;

        var mouseScreenPos = _inputManager.MouseScreenPosition;
        var mousePos = _eyeManager.PixelToMap(mouseScreenPos);

        if (mousePos.MapId != args.MapId)
            return;

        // No map support for decals
        if (!_mapManager.TryFindGridAt(mousePos, out var gridUid, out var grid))
        {
            return;
        }

        var worldMatrix = _transform.GetWorldMatrix(gridUid);
        var invMatrix = _transform.GetInvWorldMatrix(gridUid);

        var handle = args.WorldHandle;
        handle.SetTransform(worldMatrix);

        var localPos = Vector2.Transform(mousePos.Position, invMatrix);

        // Nothing uses snap cardinals so probably don't need preview?
        var aabb = Box2.UnitCentered.Translated(localPos);
        var box = new Box2Rotated(aabb, Angle.Zero, localPos);

        handle.DrawTextureRect(_sprite.Frame0(_decal.Sprite), box, _color);
        handle.SetTransform(Matrix3x2.Identity);
    }
}
