// SPDX-FileCopyrightText: 2024 August Eymann <august.eymann@gmail.com>
// SPDX-FileCopyrightText: 2024 chromiumboy <50505512+chromiumboy@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 marc-pelletier <113944176+marc-pelletier@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2024 Plykiya <58439124+Plykiya@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 taydeo <td12233a@gmail.com>
//
// SPDX-License-License: MIT

using Content.Client.Construction;
using Content.Client.Gameplay;
using Content.Shared.Atmos.Components;
using Content.Shared.Construction.Prototypes;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction;
using Content.Shared.RCD;
using Content.Shared.RCD.Components;
using Content.Shared.RCD.Systems;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Placement;
using Robust.Client.Player;
using Robust.Client.State;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Numerics;
using static Robust.Client.Placement.PlacementManager;

namespace Content.Client.RCD;

/// <summary>
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
    [Dependency] private readonly IEntityNetworkManager _entityNetwork = default!;

    private readonly SharedMapSystem _mapSystem;
    private readonly SharedTransformSystem _transformSystem;
    private readonly SpriteSystem _spriteSystem;
    private readonly RCDSystem _rcdSystem;

    private const float SearchBoxSize = 2f;
    private const float MouseDeadzoneRadius = 0.25f;
    private const float PlaceColorBaseAlpha = 0.5f;
    private const float GuideRadius = 0.1f;
    private const float GuideOffset = 0.21875f;

    private EntityCoordinates _mouseCoordsRaw = default;
    private AtmosPipeLayer _currentLayer = AtmosPipeLayer.Primary;
    private Color _guideColor = new(0, 0, 0.5785f);

    public AlignRPDAtmosPipeLayers(PlacementManager pMan) : base(pMan)
    {
        IoCManager.InjectDependencies(this);
        _mapSystem = _entityManager.System<SharedMapSystem>();
        _transformSystem = _entityManager.System<SharedTransformSystem>();
        _spriteSystem = _entityManager.System<SpriteSystem>();
        _rcdSystem = _entityManager.System<RCDSystem>();
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
        var mouseCoordsDiff = _mouseCoordsRaw.Position - MouseCoords.Position;
        var newLayer = AtmosPipeLayer.Primary;

        if (mouseCoordsDiff.Length() > MouseDeadzoneRadius)
        {
            var gridRotation = _transformSystem.GetWorldRotation(gridId.Value);
            var direction = (new Angle(mouseCoordsDiff) + _eyeManager.CurrentEye.Rotation + gridRotation + Math.PI / 2).GetCardinalDir();
            newLayer = (direction == Direction.North || direction == Direction.East) ? AtmosPipeLayer.Secondary : AtmosPipeLayer.Tertiary;
        }

        // Update the layer only if within interaction range
        if (_playerManager.LocalSession?.AttachedEntity is { } player &&
            _entityManager.TryGetComponent<TransformComponent>(player, out var xform) &&
            _transformSystem.InRange(xform.Coordinates, MouseCoords, SharedInteractionSystem.InteractionRange) &&
            _entityManager.TryGetComponent<HandsComponent>(player, out var hands) &&
            hands.ActiveHand?.HeldEntity is { } heldEntity &&
            _entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd))
        {
            _entityNetwork.SendSystemNetworkMessage(new RPDSelectLayerEvent(_entityManager.GetNetEntity(heldEntity), newLayer));
            _currentLayer = newLayer;
        }

        // Update the construction menu placer
        if (pManager.Hijack != null)
        {
            UpdateHijackedPlacer(_currentLayer, mouseScreen);
        }
        else
        {
            UpdatePlacer(_currentLayer);
        }
    }

    private void UpdateHijackedPlacer(AtmosPipeLayer layer, ScreenCoordinates mouseScreen)
    {
        var constructionSystem = (pManager.Hijack as ConstructionPlacementHijack)?.CurrentConstructionSystem;
        var altPrototypes = (pManager.Hijack as ConstructionPlacementHijack)?.CurrentPrototype?.AlternativePrototypes;

        if (constructionSystem == null || altPrototypes == null || (int)layer >= altPrototypes.Length)
            return;

        var newProtoId = altPrototypes[(int)layer];

        if (!_protoManager.TryIndex(newProtoId, out var newProto))
            return;

        if (newProto.Type != ConstructionType.Structure)
        {
            pManager.Clear();
            return;
        }

        if (newProto.ID == (pManager.Hijack as ConstructionPlacementHijack)?.CurrentPrototype?.ID)
            return;

        pManager.BeginPlacing(new PlacementInformation()
        {
            IsTile = false,
            PlacementOption = newProto.PlacementMode,
        }, new ConstructionPlacementHijack(constructionSystem, newProto));

        if (pManager.CurrentMode is AlignRPDAtmosPipeLayers { } newMode)
            newMode.RefreshGrid(mouseScreen);

        constructionSystem.GetGuide(newProto);
    }

    private void UpdatePlacer(AtmosPipeLayer layer)
    {
        if (pManager.CurrentPermission?.EntityType == null)
            return;

        if (!_protoManager.TryIndex<EntityPrototype>(pManager.CurrentPermission.EntityType, out var currentProto))
            return;

        var baseProtoId = currentProto.ID;
        if (baseProtoId.EndsWith("Alt1") || baseProtoId.EndsWith("Alt2"))
            baseProtoId = baseProtoId.Substring(0, baseProtoId.Length - 4);

        var newProtoId = layer switch
        {
            AtmosPipeLayer.Primary => baseProtoId,
            AtmosPipeLayer.Secondary => $"{baseProtoId}Alt1",
            AtmosPipeLayer.Tertiary => $"{baseProtoId}Alt2",
            _ => baseProtoId
        };

        if (_protoManager.TryIndex<EntityPrototype>(newProtoId, out var newProto))
        {
            pManager.CurrentPermission.EntityType = newProtoId;

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

    private void RefreshGrid(ScreenCoordinates mouseScreen)
    {
        MouseCoords = ScreenToCursorGrid(mouseScreen).AlignWithClosestGridTile(SearchBoxSize, _entityManager, _mapManager);

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
    }

    public override bool IsValidPosition(EntityCoordinates position)
    {
        if (_playerManager.LocalSession?.AttachedEntity is not { } player ||
            !_entityManager.TryGetComponent<TransformComponent>(player, out var xform) ||
            !_transformSystem.InRange(xform.Coordinates, position, SharedInteractionSystem.InteractionRange))
        {
            InvalidPlaceColor = InvalidPlaceColor.WithAlpha(0);
            return false;
        }

        InvalidPlaceColor = InvalidPlaceColor.WithAlpha(PlaceColorBaseAlpha);

        if (!_entityManager.TryGetComponent<HandsComponent>(player, out var hands) ||
            hands.ActiveHand?.HeldEntity is not { } heldEntity ||
            !_entityManager.TryGetComponent<RCDComponent>(heldEntity, out var rcd) ||
            !_rcdSystem.TryGetMapGridData(position, out var mapGridData) ||
            _stateManager.CurrentState is not GameplayStateBase screen)
            return false;

        var target = screen.GetClickedEntity(_transformSystem.ToMapCoordinates(_mouseCoordsRaw));
        return _rcdSystem.IsRCDOperationStillValid(heldEntity, rcd, mapGridData.Value, target, player, false);
    }
}