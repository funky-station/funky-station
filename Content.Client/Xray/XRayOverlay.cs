// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.XRay;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Linq;
using System.Numerics;

namespace Content.Client.XRay;

public sealed class XRayOverlay : Overlay
{
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IResourceCache _resource = default!;

    private readonly EntityLookupSystem _lookups;
    private readonly SpriteSystem _sprite;
    private readonly SharedTransformSystem _transform;
    private readonly ShowXRaySystem _xray;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;

    public HashSet<Entity<ShowXRayComponent>> Providers = [];

    private const float UpdateRate = 1f / 30f;
    private float _accumulator;

    private readonly Dictionary<EntityUid, HashSet<TileRef>> _tiles = [];
    private readonly Dictionary<EntityUid, List<EntityUid>> _entities = [];

    private readonly Dictionary<EntityUid, MapGridComponent> _mapGrids = [];
    private readonly Dictionary<EntityUid, SpriteComponent> _sprites = [];

    private readonly Dictionary<Tile, Dictionary<byte, Texture>> _tileVariations = [];

    public XRayOverlay()
    {
        IoCManager.InjectDependencies(this);
        _lookups = _entityManager.System<EntityLookupSystem>();
        _sprite = _entityManager.System<SpriteSystem>();
        _transform = _entityManager.System<SharedTransformSystem>();
        _xray = _entityManager.System<ShowXRaySystem>();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var eyeRot = args.Viewport.Eye?.Rotation ?? Angle.Zero;

        Dictionary<EntityUid, Matrix3x2> mapMatrix = [];

        _accumulator -= (float)_timing.FrameTime.TotalSeconds;

        if (_accumulator <= 0f)
        {
            _accumulator = MathF.Max(0f, _accumulator + UpdateRate);
            Refresh();
        }

        foreach (var xray in Providers)
        {
            var shader = _prototype.Index<ShaderPrototype>(xray.Comp.Shader).InstanceUnique();
            args.WorldHandle.UseShader(shader);

            foreach (var tileRef in _tiles[xray])
            {
                if (!_mapGrids.TryGetValue(tileRef.GridUid, out var mapGrid))
                {
                    if (!_entityManager.TryGetComponent(tileRef.GridUid, out mapGrid))
                        continue;

                    _mapGrids.Add(tileRef.GridUid, mapGrid);
                }

                var tile = _tileDef[tileRef.Tile.TypeId];

                if (tile.Sprite is not { } sprite)
                    continue;

                if (!mapMatrix.TryGetValue(tileRef.GridUid, out var transform))
                {
                    transform = _transform.GetWorldMatrix(tileRef.GridUid);
                    mapMatrix.Add(tileRef.GridUid, transform);
                }

                args.DrawingHandle.SetTransform(transform);

                var bounds = _lookups.GetLocalBounds(tileRef, mapGrid.TileSize);

                if (!_tileVariations.TryGetValue(tileRef.Tile, out var variants) || !variants.TryGetValue(tileRef.Tile.Variant, out var texture))
                {
                    var atlasTexture = _resource.GetResource<TextureResource>(sprite);

                    if (tile.Variants == 1)
                    {
                        texture = atlasTexture;
                    }
                    else
                    {
                        var variant = tileRef.Tile.Variant + 1;
                        var size = atlasTexture.Texture.Size.X / tile.Variants;

                        var variantBounds = UIBox2.FromDimensions(variant * size - size, 0, size, atlasTexture.Texture.Size.Y);
                        texture = new AtlasTexture(atlasTexture, variantBounds);
                    }

                    if (!_tileVariations.ContainsKey(tileRef.Tile))
                        _tileVariations.Add(tileRef.Tile, []);

                    _tileVariations[tileRef.Tile].Add(tileRef.Tile.Variant, texture);
                }

                // TODO: maybe get decals too?
                args.WorldHandle.DrawTextureRect(texture, bounds);
            }

            foreach (var entity in _entities[xray])
            {
                if (!_sprites.TryGetValue(entity, out var sprite))
                {
                    sprite = _entityManager.GetComponent<SpriteComponent>(entity);
                    _sprites[entity] = sprite;
                }

                var entityXform = _entityManager.GetComponent<TransformComponent>(entity);
                var (worldPos, worldRot) = _transform.GetWorldPositionRotation(entityXform);
                _sprite.RenderSprite((entity, sprite), args.WorldHandle, eyeRot, worldRot, worldPos);
            }
        }

        args.DrawingHandle.SetTransform(Matrix3x2.Identity);
        args.WorldHandle.UseShader(null);
    }

    public void Refresh()
    {
        _tiles.Clear();
        _entities.Clear();

        foreach (var xray in Providers)
        {
            _tiles[xray] = _xray.GetTiles(xray).ToHashSet();
            _entities[xray] = _xray.GetEntities(xray).OrderBy(x => _entityManager.GetComponent<SpriteComponent>(x).DrawDepth).ToList();
        }
    }
}
