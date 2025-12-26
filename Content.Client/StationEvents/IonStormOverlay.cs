// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Client.Parallax;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;
using System.Linq;
using System.Numerics;
using DrawDepth = Content.Shared.DrawDepth.DrawDepth;

namespace Content.Client.StationEvents;

public sealed class IonStormOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceEntities;

    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly ParallaxSystem _parallax;
    private readonly SpriteSystem _sprite;

    private readonly ShaderInstance _shader;
    private readonly Texture _noiseTexture;

    private static readonly ResPath NoiseTexturePath = new("/Textures/Parallaxes/noise.png");
    private const float NoiseTextureScale = 0.5f;

    private readonly HashSet<MapId> _maps = [];
    private Vector2 _direction;
    private float _speed;
    private readonly Dictionary<MapId, float> _alphas = [];

    private const float MinSpeed = 1f;
    private const float MaxSpeed = 2.5f;
    private const float FadeSpeed = 2.5f;
    private const float HiddenAlphaThreshold = 0.01f;

    public IonStormOverlay() : base()
    {
        IoCManager.InjectDependencies(this);
        _parallax = _entity.System<ParallaxSystem>();
        _sprite = _entity.System<SpriteSystem>();

        ZIndex = (int)DrawDepth.BelowWalls;

        var sprite = new SpriteSpecifier.Texture(NoiseTexturePath);
        _noiseTexture = _sprite.Frame0(sprite);

        _shader = _prototype.Index<ShaderPrototype>("IonStorm").InstanceUnique();
        _shader.SetParameter("seed", _random.NextFloat());
        NewDirection();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return IsVisible(args.MapId);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var alpha = _alphas[args.MapId];
        _shader.SetParameter("alpha", alpha);
        _shader.SetParameter("noise", _noiseTexture);

        args.WorldHandle.SetTransform(Matrix3x2.Identity);
        args.WorldHandle.UseShader(null);

        args.WorldHandle.UseShader(_shader);
        _parallax.DrawParallax(args.WorldHandle, args.WorldAABB, _noiseTexture, _timing.RealTime, Vector2.Zero, _direction * _speed, NoiseTextureScale);
        args.WorldHandle.UseShader(null);
    }

    public void AddMap(MapId map)
    {
        if (_maps.Any())
            NewDirection();

        _maps.Add(map);
        _alphas[map] = 0f;
    }

    public void RemoveMap(MapId map)
    {
        _maps.Remove(map);
        _alphas.Remove(map);
    }

    public void NewDirection()
    {
        _speed = _random.NextFloat(MinSpeed, MaxSpeed);
        _direction = _random.NextVector2().Normalized();
        _shader.SetParameter("direction", _direction);
    }

    public void UpdateFade(MapId mapId, float frameTime, bool active)
    {
        var alpha = _alphas[mapId];

        var target = active ? 1f : 0f;

        if (alpha < target)
            alpha = Math.Min(alpha + FadeSpeed * frameTime, target);
        else if (alpha > target)
            alpha = Math.Max(alpha - FadeSpeed * frameTime, target);

        _alphas[mapId] = alpha;
    }

    public bool IsVisible(MapId mapId) => _alphas.TryGetValue(mapId, out var alpha) && alpha > HiddenAlphaThreshold;
}
