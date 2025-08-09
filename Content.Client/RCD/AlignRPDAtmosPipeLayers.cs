using Content.Client.Gameplay;
using Content.Shared.Atmos.Components;
using Content.Shared.Atmos.EntitySystems;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;
using static Robust.Client.Placement.PlacementManager;

namespace Content.Client.RCD;

/// <summary>
/// Funkystation
/// Allows users to place RCD prototypes with atmos pipe layers on different layers depending on how the mouse cursor is positioned within a grid tile.
/// </summary>
/// <remarks>
/// This placement mode is not on the engine because it is content specific.
/// </remarks>
public sealed class AlignRPDAtmosPipeLayers : PlacementMode
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IPrototypeManager _protoManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IStateManager _stateManager = default!;
    [Dependency] private readonly IEyeManager _eyeManager = default!;

    private readonly SharedMapSystem _mapSystem;
    private readonly SharedTransformSystem _transformSystem;
    private readonly SharedAtmosPipeLayersSystem _pipeLayersSystem;
    private readonly SpriteSystem _spriteSystem;
    private readonly RCDSystem _rcdSystem;

    private const float SearchBoxSize = 2f;
    private const float MouseDeadzoneRadius = 0.25f;
    private const float PlaceColorBaseAlpha = 0.5f;
    private const float GuideRadius = 0.1f;
    private const float GuideOffset = 0.21875f;

    private EntityCoordinates _mouseCoordsRaw = default;
    private static AtmosPipeLayer _currentLayer = AtmosPipeLayer.Primary;
    private Color _guideColor = new(0, 0, 0.5785f);

    public AlignRPDAtmosPipeLayers(PlacementManager pMan) : base(pMan)
    {
        IoCManager.InjectDependencies(this);
        _mapSystem = _entityManager.System<SharedMapSystem>();
        _transformSystem = _entityManager.System<SharedTransformSystem>();
        _spriteSystem = _entityManager.System<SpriteSystem>();
        _rcdSystem = _entityManager.System<RCDSystem>();
        _pipeLayersSystem = _entityManager.System<SharedAtmosPipeLayersSystem>();
        ValidPlaceColor = ValidPlaceColor.WithAlpha(PlaceColorBaseAlpha);
    }

    public override void Render(in OverlayDrawArgs args)
    {
        // Early exit if mouse is out of interaction range
        if (_playerManager.LocalSession?.AttachedEntity is not { } player ||
            !_entityManager.TryGetComponent<TransformComponent>(player, out var xform) ||
            !_transformSystem.InRange(xform.Coordinates, MouseCoords, SharedInteractionSystem.InteractionRange))
        {
            return;
        }

        var gridUid = _transformSystem.GetGrid(MouseCoords);

        if (gridUid == null || !_entityManager.TryGetComponent<MapGridComponent>(gridUid, out var grid))
            return;

        // Draw guide circles for each pipe layer if we are not in line/grid placing mode
        if (pManager.PlacementType == PlacementTypes.None)
        {
            var gridRotation = _transformSystem.GetWorldRotation(gridUid.Value);
            var worldPosition = _mapSystem.LocalToWorld(gridUid.Value, grid, MouseCoords.Position);
            var direction = (_eyeManager.CurrentEye.Rotation + gridRotation + Math.PI / 2).GetCardinalDir();
            var multi = (direction == Direction.North || direction == Direction.South) ? -1f : 1f;

            args.WorldHandle.DrawCircle(worldPosition, GuideRadius, _guideColor);
            args.WorldHandle.DrawCircle(worldPosition + gridRotation.RotateVec(new Vector2(multi * GuideOffset, GuideOffset)), GuideRadius, _guideColor);
            args.WorldHandle.DrawCircle(worldPosition - gridRotation.RotateVec(new Vector2(multi * GuideOffset, GuideOffset)), GuideRadius, _guideColor);
        }

        base.Render(args);
    }

    public override void AlignPlacementMode(ScreenCoordinates mouseScreen)
    {
        _mouseCoordsRaw = ScreenToCursorGrid(mouseScreen);
        MouseCoords = _mouseCoordsRaw.AlignWithClosestGridTile(SearchBoxSize, _entityManager, _mapManager);

        var gridId = _transformSystem.GetGrid(MouseCoords);

        if (!_entityManager.TryGetComponent<MapGridComponent>(gridId, out var mapGrid))
            return;

        CurrentTile = _mapSystem.GetTileRef(gridId.Value, mapGrid, MouseCoords);

        float tileSize = mapGrid.TileSize;
        GridDistancing = tileSize;

        if (pManager.CurrentPermission!.IsTile)
        {
            MouseCoords = new EntityCoordinates(MouseCoords.EntityId, new Vector2(CurrentTile.X + tileSize / 2,
                CurrentTile.Y + tileSize / 2));
        }
        else
        {
            MouseCoords = new EntityCoordinates(MouseCoords.EntityId, new Vector2(CurrentTile.X + tileSize / 2 + pManager.PlacementOffset.X,
                CurrentTile.Y + tileSize / 2 + pManager.PlacementOffset.Y));
        }

        // Calculate the position of the mouse cursor with respect to the center of the tile to determine which layer to use
        // For the time being this cannot rotate as there is no way to easily get client rotation data or otherwise 
        // send the layer to the server. This same check has to be done on the server as well sans client rotation data.
        // Ghosts accurately reflect final placement of prototypes.
        var mouseCoordsDiff = _mouseCoordsRaw.Position - MouseCoords.Position;
        var newLayer = AtmosPipeLayer.Primary;
        if (Math.Abs(mouseCoordsDiff.X) > MouseDeadzoneRadius || Math.Abs(mouseCoordsDiff.Y) > MouseDeadzoneRadius)
        {
            var angle = new Angle(new Vector2(mouseCoordsDiff.X, mouseCoordsDiff.Y)) + Math.PI / 2;
            var direction = angle.GetCardinalDir();
            newLayer = (direction == Direction.North || direction == Direction.East) ? AtmosPipeLayer.Secondary : AtmosPipeLayer.Tertiary;
        }

        // Update the layer only if within interaction range and layer has changed
        if (_playerManager.LocalSession?.AttachedEntity is { } player &&
            _entityManager.TryGetComponent<TransformComponent>(player, out var xform) &&
            _transformSystem.InRange(xform.Coordinates, MouseCoords, SharedInteractionSystem.InteractionRange) &&
            _entityManager.TryGetComponent<HandsComponent>(player, out var hands) &&
            hands.ActiveHand?.HeldEntity is { } heldEntity &&
            _entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd) &&
            newLayer != _currentLayer)
        {
            _currentLayer = newLayer;
        }

        UpdatePlacer(_currentLayer);
    }

    private void UpdatePlacer(AtmosPipeLayer layer)
    {
        // Try to get alternative prototypes from the entity atmos pipe layer component
        if (pManager.CurrentPermission?.EntityType == null)
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(pManager.CurrentPermission.EntityType, out var currentProto))
            return;

        if (!currentProto.TryGetComponent<AtmosPipeLayersComponent>(out var atmosPipeLayers, _entityManager.ComponentFactory))
            return;

        if (!_pipeLayersSystem.TryGetAlternativePrototype(atmosPipeLayers, layer, out var newProtoId))
            return;

        if (_protoManager.TryIndex<EntityPrototype>(newProtoId, out var newProto))
        {
            // Update the placed prototype
            pManager.CurrentPermission.EntityType = newProtoId;

            // Update the appearance of the ghost sprite
            if (newProto.TryGetComponent<SpriteComponent>(out var sprite, _entityManager.ComponentFactory))
            {
                var textures = new List<IDirectionalTextureProvider>();

                foreach (var spriteLayer in sprite.AllLayers)
                {
                    if (spriteLayer.ActualRsi?.Path != null && spriteLayer.RsiState.Name != null)
                        textures.Add(_spriteSystem.RsiStateLike(new SpriteSpecifier.Rsi(spriteLayer.ActualRsi.Path, spriteLayer.RsiState.Name)));
                }

                pManager.CurrentTextures = textures;
            }
        }
    }

    public override bool IsValidPosition(EntityCoordinates position)
    {
        var player = _playerManager.LocalSession?.AttachedEntity;

        // If the destination is out of interaction range, set the placer alpha to zero
        if (!_entityManager.TryGetComponent<TransformComponent>(player, out var xform))
            return false;

        if (!_transformSystem.InRange(xform.Coordinates, position, SharedInteractionSystem.InteractionRange))
        {
            InvalidPlaceColor = InvalidPlaceColor.WithAlpha(0);
            return false;
        }

        // Otherwise restore the alpha value
        else
        {
            InvalidPlaceColor = InvalidPlaceColor.WithAlpha(PlaceColorBaseAlpha);
        }

        // Determine if player is carrying an RCD in their active hand
        if (!_entityManager.TryGetComponent<HandsComponent>(player, out var hands))
            return false;

        var heldEntity = hands.ActiveHand?.HeldEntity;

        if (!_entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd))
            return false;

        // Retrieve the map grid data for the position
        if (!_rcdSystem.TryGetMapGridData(position, out var mapGridData))
            return false;

        // Determine if the user is hovering over a target
        var currentState = _stateManager.CurrentState;

        if (currentState is not GameplayStateBase screen)
            return false;

        var target = screen.GetClickedEntity(_transformSystem.ToMapCoordinates(_mouseCoordsRaw));

        // Determine if the RCD operation is valid or not
        if (!_rcdSystem.IsRCDOperationStillValid(heldEntity.Value, rcd, mapGridData.Value, target, player.Value, false))
            return false;

        return true;
    }
}