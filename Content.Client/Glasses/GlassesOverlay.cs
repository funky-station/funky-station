// SPDX-FileCopyrightText: 2025 YaraaraY <158123176+YaraaraY@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Glasses;
using Robust.Client.Graphics;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Client.Glasses;

public sealed class GlassesOverlay : Overlay
{
    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    public override bool RequestScreenTexture => true;

    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public HashSet<GlassesOverlayComponent> Providers = new();

    public GlassesOverlay()
    {
        IoCManager.InjectDependencies(this);
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        return Providers.Any();
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (ScreenTexture == null)
            return;

        var worldHandle = args.WorldHandle;
        var worldBounds = args.WorldBounds;

        foreach (var provider in Providers)
        {
            var shader = _prototype.Index<ShaderPrototype>(provider.Shader).InstanceUnique();
            shader.SetParameter("color", provider.Color);
            shader.SetParameter("SCREEN_TEXTURE", ScreenTexture);

            worldHandle.UseShader(shader);
            worldHandle.DrawRect(worldBounds, Color.White);
        }

        worldHandle.UseShader(null);
    }
}
