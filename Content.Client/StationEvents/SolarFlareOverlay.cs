// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Client.Parallax;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Client.StationEvents;

public sealed class SolarFlareOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

    private const float OffsetMinMagnitude = 1.414213562f;
    private const float OffsetMaxMagnitude = 1.5f;
    private const float FadeSpeed = 0.5f;
    private const float HiddenAlphaThreshold = 0.01f;

    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private readonly ShaderInstance _shader;

    private float _alpha;
    public SolarFlareVisualsFadeState FadeState { get; private set; }

    public SolarFlareOverlay() : base()
    {
        IoCManager.InjectDependencies(this);

        ZIndex = ParallaxSystem.ParallaxZIndex + 1;

        _shader = _prototype.Index<ShaderPrototype>("SolarFlare").InstanceUnique();
        _shader.SetParameter("seed", _random.NextFloat());
        _shader.SetParameter("offset", _random.NextVector2(OffsetMinMagnitude, OffsetMaxMagnitude));
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return args.MapId != MapId.Nullspace;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        _shader.SetParameter("alpha", _alpha);

        args.WorldHandle.SetTransform(Matrix3x2.Identity);
        args.WorldHandle.UseShader(_shader);
        args.WorldHandle.DrawRect(args.WorldBounds, Color.White);
        args.WorldHandle.UseShader(null);
    }

    public void UpdateAlpha(float frameTime)
    {
        switch (FadeState)
        {
            case SolarFlareVisualsFadeState.FadeIn:
                _alpha += FadeSpeed * frameTime;
                if (_alpha >= 1f)
                {
                    _alpha = 1f;
                    FadeState = SolarFlareVisualsFadeState.None;
                }
                break;
            case SolarFlareVisualsFadeState.FadeOut:
                _alpha -= FadeSpeed * frameTime;
                if (_alpha <= 0f)
                {
                    _alpha = 0f;
                    FadeState = SolarFlareVisualsFadeState.None;
                }
                break;
        }
    }

    public void StartFadeIn()
    {
        FadeState = SolarFlareVisualsFadeState.FadeIn;
    }

    public void StartFadeOut()
    {
        FadeState = SolarFlareVisualsFadeState.FadeOut;
    }

    public bool IsVisible() => _alpha > HiddenAlphaThreshold;
}
